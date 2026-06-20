using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class VisitService : IVisitService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<VisitService> _logger;

    public VisitService(FinalLabDbContext context, ILogger<VisitService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            visit.VisitStatus = VisitStatus.Open;
            visit.PaymentStatus = PaymentStatus.Pending;
            visit.Subtotal = 0;
            visit.DiscountAmount = 0;
            visit.DiscountPercent = 0;
            visit.TotalAfterDiscount = 0;
            visit.TotalPaid = 0;
            visit.BalanceDue = 0;

            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            var allTestTypeIds = new HashSet<int>(testIds);

            if (profileIds.Count > 0)
            {
                var profiles = await _context.TestProfiles
                    .Include(p => p.TestProfileItems)
                        .ThenInclude(tpi => tpi.TestType)
                    .Where(p => profileIds.Contains(p.ProfileId))
                    .ToListAsync();

                foreach (var profile in profiles)
                {
                    foreach (var item in profile.TestProfileItems)
                    {
                        allTestTypeIds.Add(item.TestTypeId);
                    }
                }
            }

            var testTypes = await _context.TestTypes
                .Where(tt => allTestTypeIds.Contains(tt.TesttypeId))
                .ToListAsync();

            var testTypesDict = testTypes.ToDictionary(tt => tt.TesttypeId);

            foreach (var testTypeId in allTestTypeIds)
            {
                if (testTypesDict.TryGetValue(testTypeId, out var testType))
                {
                    var visitTest = new VisitTest
                    {
                        VisitId = visit.VisitId,
                        TesttypeId = testTypeId,
                        PriceCharged = testType.DefaultPrice,
                        CurrentStage = TestStage.Pending,
                        IsOutsourced = false,
                        AddedAt = DateTime.UtcNow
                    };
                    _context.VisitTests.Add(visitTest);
                }
            }

            foreach (var charge in charges)
            {
                charge.VisitId = visit.VisitId;
                _context.VisitCharges.Add(charge);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await _context.Visits
                .Include(v => v.Patient)
                .Include(v => v.VisitTests)
                    .ThenInclude(vt => vt.Testtype)
                .Include(v => v.VisitCharges)
                .Include(v => v.Scheme)
                .Include(v => v.Receptionist)
                .FirstAsync(v => v.VisitId == visit.VisitId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Visit> SavePatientVisitAsync(
        Patient patient,
        Visit visit,
        List<int> testTypeIds,
        decimal amountPaid,
        int staffId,
        List<PatientMedicalHistory> medicalHistories,
        ReferralSource? referralToSave)
    {
        _context.ChangeTracker.Clear();
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (referralToSave is not null)
            {
                referralToSave.CreatedAt = referralToSave.CreatedAt == default ? DateTime.UtcNow : referralToSave.CreatedAt;
                referralToSave.IsActive = true;
                _context.ReferralSources.Add(referralToSave);
                await _context.SaveChangesAsync();
                visit.ReferralId = referralToSave.ReferralId;
            }

            if (patient.PatientId == 0)
            {
                patient.CreatedAt = patient.CreatedAt == default ? DateTime.UtcNow : patient.CreatedAt;
                patient.PatientType = string.IsNullOrWhiteSpace(patient.PatientType) ? "Individual" : patient.PatientType;
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Patients.Update(patient);
                await _context.SaveChangesAsync();
            }

            var uniqueTestIds = testTypeIds.Distinct().ToList();
            var testTypes = await _context.TestTypes
                .Where(t => uniqueTestIds.Contains(t.TesttypeId))
                .ToListAsync();

            var subtotal = testTypes.Sum(t => t.DefaultPrice);
            visit.PatientId = patient.PatientId;
            visit.Subtotal = subtotal;
            visit.DiscountAmount = Math.Clamp(visit.DiscountAmount, 0, subtotal);
            visit.DiscountPercent = subtotal <= 0 ? 0 : Math.Round(visit.DiscountAmount / subtotal * 100, 2);
            visit.TotalAfterDiscount = Math.Max(0, subtotal - visit.DiscountAmount);
            visit.TotalPaid = amountPaid;
            visit.BalanceDue = visit.TotalAfterDiscount - visit.TotalPaid;
            visit.PaymentStatus = visit.BalanceDue <= 0 ? PaymentStatus.Paid : visit.TotalPaid > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Pending;
            visit.VisitStatus = VisitStatus.Open;
            visit.CreatedAt = visit.CreatedAt == default ? DateTime.UtcNow : visit.CreatedAt;
            visit.UpdatedAt = DateTime.UtcNow;

            if (visit.VisitId == 0)
            {
                _context.Visits.Add(visit);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Visits.Update(visit);
                await _context.SaveChangesAsync();
            }

            await UpdateVisitTestsInternalAsync(visit.VisitId, uniqueTestIds);

            var oldHistory = await _context.PatientMedicalHistories
                .Where(h => h.PatientId == patient.PatientId)
                .ToListAsync();
            _context.PatientMedicalHistories.RemoveRange(oldHistory);

            foreach (var history in medicalHistories)
            {
                history.PatientId = patient.PatientId;
                history.CreatedAt = history.CreatedAt == default ? DateTime.UtcNow : history.CreatedAt;
                history.CreatedBy = staffId;
                history.IsActive = true;
                _context.PatientMedicalHistories.Add(history);
            }

            var oldPayments = await _context.Payments
                .Where(p => p.VisitId == visit.VisitId)
                .ToListAsync();
            _context.Payments.RemoveRange(oldPayments);

            if (amountPaid > 0)
            {
                _context.Payments.Add(new Payment
                {
                    VisitId = visit.VisitId,
                    PaymentDate = DateTime.UtcNow,
                    Amount = amountPaid,
                    PaymentMethod = PaymentMethod.Cash,
                    PaymentType = "PAYMENT",
                    ReceivedBy = staffId,
                    Notes = "Patient registration payment"
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetVisitSummaryAsync(visit.VisitId))!;
        }
        catch (Exception originalEx)
        {
            _logger.LogError(originalEx, "Error occurred while saving patient visit");
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx, "Rollback failed (original error will be re-thrown)");
            }
            finally
            {
                _context.ChangeTracker.Clear();
            }
            throw;
        }
    }

    public async Task<VisitFullDto> GetVisitFullDataAsync(int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Referral)
            .Include(v => v.Payments)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.Testtype)
                    .ThenInclude(t => t.Group)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit is null)
            throw new InvalidOperationException($"Visit with ID {visitId} not found.");

        var totalPaid = visit.Payments.Sum(payment => payment.Amount);
        var balanceDue = Math.Max(0, visit.TotalAfterDiscount - totalPaid);

        return new VisitFullDto
        {
            PatientId = visit.PatientId,
            VisitId = visit.VisitId,
            PatientCode = visit.Patient.PatientCode,
            FullNameAr = visit.Patient.FullNameAr,
            Title = visit.Patient.Title,
            Sex = string.IsNullOrWhiteSpace(visit.Patient.Sex) ? "U" : visit.Patient.Sex,
            PatientType = string.IsNullOrWhiteSpace(visit.Patient.PatientType) ? "Individual" : visit.Patient.PatientType,
            IsVip = visit.Patient.IsVip,
            ApproxAge = visit.Patient.ApproxAge,
            ApproxAgeUnit = string.IsNullOrWhiteSpace(visit.Patient.ApproxAgeUnit) ? "Years" : visit.Patient.ApproxAgeUnit,
            Phone = visit.Patient.Phone,
            Phone2 = visit.Patient.Phone2,
            Address = visit.Patient.Address,
            Email = visit.Patient.Email,
            NationalId = visit.Patient.NationalId,
            Notes = visit.Patient.Notes,
            EntryDate = visit.VisitDate,
            ExpectedReady = visit.ExpectedReady,
            ReferralId = visit.ReferralId,
            ReferralTitle = visit.Referral?.Title,
            ReferralName = visit.Referral?.SourceName,
            ReferralAddress = visit.Referral?.Address,
            IsFasting = visit.IsFasting,
            FastingHours = visit.FastingHours,
            IsPregnant = visit.IsPregnant,
            VisitNotes = visit.Notes,
            TakenOutsideLab = visit.TakenOutsideLab,
            OutsideUrine = visit.OutsideUrine,
            OutsideStool = visit.OutsideStool,
            OutsideBlood = visit.OutsideBlood,
            OutsideSemen = visit.OutsideSemen,
            OutsideCsf = visit.OutsideCsf,
            HasDiabetes = visit.HasDiabetes,
            HasAnemia = visit.HasAnemia,
            HasBleedingDisorder = visit.HasBleedingDisorder,
            HasThyroid = visit.HasThyroid,
            HasJointDisease = visit.HasJointDisease,
            HasViralInfection = visit.HasViralInfection,
            OnAnticoagulant = visit.OnAnticoagulant,
            HasHypertension = visit.HasHypertension,
            HasLiverDisease = visit.HasLiverDisease,
            HasKidneyDisease = visit.HasKidneyDisease,
            HasLupus = visit.HasLupus,
            HadXrayContrast = visit.HadXrayContrast,
            SelectedTests = visit.VisitTests
                .OrderBy(vt => vt.VisitTestId)
                .Select(vt => new SelectedTestDto
                {
                    TestTypeId = vt.TesttypeId,
                    TestCode = vt.Testtype.TypeCode,
                    TestName = vt.Testtype.TypeNameAr ?? vt.Testtype.TypeNameEn,
                    BillNameLine1 = vt.Testtype.BillNameLine1,
                    BillNameLine2 = vt.Testtype.BillNameLine2,
                    Price = vt.PriceCharged,
                    SampleType = vt.Testtype.SampleType,
                    GroupId = vt.Testtype.GroupId,
                    GroupName = vt.Testtype.Group != null
                        ? (vt.Testtype.Group.GroupNameAr ?? vt.Testtype.Group.GroupNameEn)
                        : null
                })
                .ToList(),
            Subtotal = visit.Subtotal,
            DiscountAmount = visit.DiscountAmount,
            DiscountPercent = visit.DiscountPercent,
            TotalAfterDiscount = visit.TotalAfterDiscount,
            TotalPaid = totalPaid,
            BalanceDue = balanceDue,
            PaymentStatus = visit.PaymentStatus.ToString()
        };
    }

    public async Task<List<TodayPatientDto>> GetTodayPatientListAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.Visits
            .Include(v => v.Patient)
            .Where(v => v.VisitDate >= today && v.VisitDate < tomorrow)
            .OrderByDescending(v => v.CreatedAt)
            .ThenByDescending(v => v.VisitId)
            .Select(v => new TodayPatientDto
            {
                PatientId = v.PatientId,
                VisitId = v.VisitId,
                PatientCode = v.Patient.PatientCode,
                FullNameAr = v.Patient.FullNameAr
            })
            .ToListAsync();
    }

    public async Task<bool> CancelVisitAsync(int visitId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var visit = await _context.Visits
                .Include(v => v.VisitTests)
                .Include(v => v.Payments)
                .Include(v => v.SampleTubes)
                .Include(v => v.VisitCharges)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit is null)
                return false;

            _context.VisitTests.RemoveRange(visit.VisitTests);
            _context.Payments.RemoveRange(visit.Payments);
            _context.SampleTubes.RemoveRange(visit.SampleTubes);
            _context.VisitCharges.RemoveRange(visit.VisitCharges);
            _context.Visits.Remove(visit);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Visit>> GetTodayVisitsWithPatientsAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Referral)
            .Include(v => v.Payments)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.Testtype)
            .Where(v => v.VisitDate >= today && v.VisitDate < tomorrow)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();
    }

    public async Task UpdateVisitTestsAsync(int visitId, List<int> testTypeIds)
    {
        await UpdateVisitTestsInternalAsync(visitId, testTypeIds.Distinct().ToList());
        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateVisitCodeAsync()
    {
        var todayPrefix = $"V{DateTime.UtcNow:yyyyMMdd}";
        var lastCode = await _context.Visits
            .Where(v => v.VisitCode.StartsWith(todayPrefix))
            .OrderByDescending(v => v.VisitCode)
            .Select(v => v.VisitCode)
            .FirstOrDefaultAsync();

        var next = 1;
        if (!string.IsNullOrWhiteSpace(lastCode) && lastCode.Length > todayPrefix.Length)
        {
            var suffix = lastCode[todayPrefix.Length..];
            if (int.TryParse(suffix, out var parsed))
                next = parsed + 1;
        }

        string candidate;
        do
        {
            candidate = $"{todayPrefix}{next:0000}";
            next++;
        }
        while (await _context.Visits.AnyAsync(v => v.VisitCode == candidate));

        return candidate;
    }

    public async Task CancelVisitTestAsync(int visitTestId)
    {
        var visitTest = await _context.VisitTests.FindAsync(visitTestId);
        if (visitTest != null)
        {
            visitTest.CurrentStage = TestStage.Cancelled;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Visit?> GetVisitSummaryAsync(int visitId)
    {
        return await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Referral)
            .Include(v => v.Payments)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.Testtype)
                    .ThenInclude(t => t.TestComponents)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.TestResults)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.TestWorkflows)
            .Include(v => v.VisitCharges)
            .Include(v => v.Scheme)
            .Include(v => v.Receptionist)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);
    }

    public async Task<List<TodayPatientWithStatusDto>> GetTodayPatientsWithStatusAsync(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        var nextDay = targetDate.AddDays(1);

        var visits = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Referral)
            .Include(v => v.Payments)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.TestResults)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.TestWorkflows)
            .Where(v => v.VisitDate >= targetDate && v.VisitDate < nextDay)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();

        var patientVisitCounts = visits
            .GroupBy(v => v.PatientId)
            .ToDictionary(g => g.Key, g => g.Count());

        return visits.Select(v =>
        {
            var status = ComputeVisitStatus(v);
            return new TodayPatientWithStatusDto
            {
                PatientId = v.PatientId,
                VisitId = v.VisitId,
                VisitCode = v.VisitCode,
                PatientCode = v.Patient.PatientCode,
                FullNameAr = v.Patient.FullNameAr,
                Title = v.Patient.Title,
                Sex = v.Patient.Sex,
                ApproxAge = v.Patient.ApproxAge,
                ApproxAgeUnit = v.Patient.ApproxAgeUnit,
                IsVip = v.Patient.IsVip,
                ReferralName = v.Referral?.SourceName,
                VisitCount = patientVisitCounts.GetValueOrDefault(v.PatientId, 1),
                ComputedStatus = status,
                StatusIcon = GetStatusIcon(status),
                StatusColor = GetStatusColor(status),
                BalanceDue = v.BalanceDue,
                PaymentStatus = v.PaymentStatus,
                VisitNotes = v.Notes
            };
        }).ToList();
    }

    public async Task<int> GetPatientVisitCountAsync(int patientId)
    {
        return await _context.Visits.CountAsync(v => v.PatientId == patientId);
    }

    private static PatientVisitStatus ComputeVisitStatus(Visit visit)
    {
        if (visit.VisitStatus == VisitStatus.Closed && visit.BalanceDue <= 0)
            return PatientVisitStatus.FullyComplete;

        if (visit.VisitStatus == VisitStatus.Closed && visit.BalanceDue > 0)
            return PatientVisitStatus.CompleteWithBalance;

        var visitTests = visit.VisitTests.ToList();
        if (visitTests.Count == 0 || visitTests.All(vt => vt.CurrentStage == TestStage.Pending))
            return PatientVisitStatus.NewNoResults;

        var allResults = visitTests
            .SelectMany(vt => vt.TestResults)
            .ToList();

        if (allResults.Count == 0 || allResults.All(r => r.ValidationStatus == ResultValidationStatus.Entered))
        {
            if (visitTests.Any(vt => vt.CurrentStage == TestStage.Pending))
                return PatientVisitStatus.HasUnwrittenResults;
            return PatientVisitStatus.HasUnreviewedResults;
        }

        if (allResults.All(r => r.ValidationStatus >= ResultValidationStatus.Reviewed))
        {
            if (visit.VisitStatus == VisitStatus.Open)
                return PatientVisitStatus.HasUndeliveredResults;
        }

        if (allResults.Any(r => r.ValidationStatus == ResultValidationStatus.Entered))
            return PatientVisitStatus.HasUnreviewedResults;

        return PatientVisitStatus.HasUnprintedResults;
    }

    private static string GetStatusIcon(PatientVisitStatus status) => status switch
    {
        PatientVisitStatus.NewNoResults => "○",
        PatientVisitStatus.HasUnwrittenResults => "◐",
        PatientVisitStatus.HasUnreviewedResults => "◑",
        PatientVisitStatus.HasUnprintedResults => "◉",
        PatientVisitStatus.HasUndeliveredResults => "◎",
        PatientVisitStatus.CompleteWithBalance => "$",
        PatientVisitStatus.FullyComplete => "✓",
        _ => "○"
    };

    private static string GetStatusColor(PatientVisitStatus status) => status switch
    {
        PatientVisitStatus.NewNoResults => "#808080",
        PatientVisitStatus.HasUnwrittenResults => "#FF8C00",
        PatientVisitStatus.HasUnreviewedResults => "#FFD700",
        PatientVisitStatus.HasUnprintedResults => "#4FC3F7",
        PatientVisitStatus.HasUndeliveredResults => "#9C27B0",
        PatientVisitStatus.CompleteWithBalance => "#F44336",
        PatientVisitStatus.FullyComplete => "#4CAF50",
        _ => "#808080"
    };

    private async Task UpdateVisitTestsInternalAsync(int visitId, List<int> testTypeIds)
    {
        var existing = await _context.VisitTests
            .Where(vt => vt.VisitId == visitId)
            .ToListAsync();

        var existingIds = existing.Select(vt => vt.TesttypeId).ToHashSet();
        var desiredIds = testTypeIds.ToHashSet();

        var toRemove = existing.Where(vt => !desiredIds.Contains(vt.TesttypeId)).ToList();
        _context.VisitTests.RemoveRange(toRemove);

        var toAdd = desiredIds.Except(existingIds).ToList();
        var tests = await _context.TestTypes
            .Where(t => toAdd.Contains(t.TesttypeId))
            .ToListAsync();

        foreach (var test in tests)
        {
            _context.VisitTests.Add(new VisitTest
            {
                VisitId = visitId,
                TesttypeId = test.TesttypeId,
                PriceCharged = test.DefaultPrice,
                CurrentStage = TestStage.Pending,
                IsOutsourced = false,
                AddedAt = DateTime.UtcNow
            });
        }
    }
}
