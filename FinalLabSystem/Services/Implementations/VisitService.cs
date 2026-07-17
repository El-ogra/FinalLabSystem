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
    private readonly IAuthService _authService;

    public VisitService(FinalLabDbContext context, ILogger<VisitService> logger, IAuthService authService)
    {
        _context = context;
        _logger = logger;
        _authService = authService;
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
                        // [VS-02] تسعير ثنائي وفق BillingType للزيارة.
                        PriceCharged = ResolveBasePriceForBilling(testType, visit.BillingType),
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
        ReferralSource? referralToSave,
        List<VisitCharge>? extraCharges = null)
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
            else if (visit.ReferralId is null)
            {
                const string defaultName = "بدون جهة";
                var defaultReferral = await _context.ReferralSources
                    .FirstOrDefaultAsync(r => r.SourceName == defaultName && r.IsActive);
                if (defaultReferral is null)
                {
                    defaultReferral = new ReferralSource
                    {
                        SourceType = "Default",
                        Category = ReferringEntityCategory.ReferralOrContractEntity,
                        SourceName = defaultName,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ReferralSources.Add(defaultReferral);
                    await _context.SaveChangesAsync();
                }
                visit.ReferralId = defaultReferral.ReferralId;
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

            // [VS-02] Subtotal يحترم ثنائية التسعير وفق BillingType للزيارة.
            var subtotal = testTypes.Sum(t => ResolveBasePriceForBilling(t, visit.BillingType));
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

            // VS-07: حفظ الرسوم الإضافية وإعادة حساب الإجمالي
            if (extraCharges is not null)
            {
                var oldCharges = await _context.VisitCharges
                    .Where(c => c.VisitId == visit.VisitId)
                    .ToListAsync();
                _context.VisitCharges.RemoveRange(oldCharges);

                foreach (var charge in extraCharges)
                {
                    charge.VisitId = visit.VisitId;
                    charge.CreatedBy = staffId;
                    charge.CreatedAt = DateTime.UtcNow;
                    charge.ChargeId = 0;
                    _context.VisitCharges.Add(charge);
                }

                var chargesTotal = extraCharges.Sum(c => c.Amount);
                visit.Subtotal = subtotal + chargesTotal;
                visit.DiscountAmount = Math.Clamp(visit.DiscountAmount, 0, visit.Subtotal);
                visit.TotalAfterDiscount = Math.Max(0, visit.Subtotal - visit.DiscountAmount);
                visit.BalanceDue = visit.TotalAfterDiscount - visit.TotalPaid;
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
            VisitCode = visit.VisitCode,
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
            LabId = visit.Patient.LabId,
            Notes = visit.Patient.Notes,
            EntryDate = visit.VisitDate,
            ExpectedReady = visit.ExpectedReady,
            ReferralId = visit.ReferralId,
            ReferralTitle = visit.Referral?.Title,
            ReferralName = visit.Referral?.SourceName,
            ReferralAddress = visit.Referral?.Address,
            BillingType = visit.BillingType,
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
            PaymentStatus = visit.PaymentStatus.ToString(),
            VisitCount = await _context.Visits.CountAsync(v => v.PatientId == visit.PatientId)
        };
    }

    public async Task<List<TodayPatientDto>> GetTodayPatientListAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.Visits
            .Include(v => v.Patient)
            .Where(v => v.VisitDate >= today && v.VisitDate < tomorrow
                     && v.VisitStatus != VisitStatus.Cancelled)
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

    public async Task<bool> CancelVisitAsync(int visitId, int staffId)
    {
        var hasPermission = await _authService.HasPermissionAsync(staffId, "VISITS.CANCEL");
        if (!hasPermission)
            throw new UnauthorizedAccessException(
                "ليس لديك صلاحية لإلغاء الزيارة. يُرجى التواصل مع المسؤول.");

        var visit = await _context.Visits.FindAsync(visitId);
        if (visit is null)
            return false;

        visit.VisitStatus = VisitStatus.Cancelled;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
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
            .Where(v => v.VisitDate >= today && v.VisitDate < tomorrow
                     && v.VisitStatus != VisitStatus.Cancelled)
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
            .Where(v => v.VisitDate >= targetDate && v.VisitDate < nextDay
                     && v.VisitStatus != VisitStatus.Cancelled)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();

        var patientVisitCounts = visits
            .GroupBy(v => v.PatientId)
            .ToDictionary(g => g.Key, g => g.Count());

        var orderedVisits = visits.OrderBy(v => v.VisitDate).ToList();

        return orderedVisits.Select((v, index) =>
        {
            var status = v.VisitDisplayStatus;
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
                PhotoPath = v.Patient.PhotoPath,
                ReferralName = v.Referral?.SourceName,
                VisitCount = patientVisitCounts.GetValueOrDefault(v.PatientId, 1),
                ComputedStatus = status,
                StatusIcon = GetStatusIcon(status),
                StatusColor = GetStatusColor(status),
                BalanceDue = v.BalanceDue,
                PaymentStatus = v.PaymentStatus,
                VisitNotes = v.Notes,
                PatientType = v.Patient.PatientType,
                AttendanceNumber = index + 1
            };
        }).ToList();
    }

    public async Task<int> GetPatientVisitCountAsync(int patientId)
    {
        return await _context.Visits.CountAsync(v => v.PatientId == patientId);
    }

    public async Task UpdateVisitNotesAsync(int visitId, string? notes)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit != null)
        {
            visit.Notes = notes;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateVisitFlagsAsync(int visitId)
    {
        var visit = await _context.Visits
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.Testtype)
                    .ThenInclude(tt => tt.TestComponents)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.TestResults)
            .Include(v => v.DeliveryConfirmations)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);

        if (visit == null) return;

        int totalComponents = visit.VisitTests.Sum(vt => vt.Testtype?.TestComponents.Count ?? 0);
        int enteredResults = visit.VisitTests.SelectMany(vt => vt.TestResults ?? Enumerable.Empty<TestResult>()).Count(r => r.ResultNumeric.HasValue || !string.IsNullOrWhiteSpace(r.ResultValue));

        visit.IsEntered = totalComponents > 0 && enteredResults >= totalComponents;

        bool allEnteredAndReviewed = visit.IsEntered && visit.VisitTests
            .SelectMany(vt => vt.TestResults ?? Enumerable.Empty<TestResult>())
            .All(r => r.ValidationStatus >= ResultValidationStatus.Reviewed);

        visit.IsReviewed = allEnteredAndReviewed;

        bool allTestsPrinted = visit.VisitTests.Count > 0 && visit.VisitTests.All(vt => vt.IsPrinted);
        visit.IsPrinted = allTestsPrinted;

        bool isDelivered = visit.DeliveryConfirmations.Count > 0;
        visit.IsDelivered = isDelivered;

        visit.IsFullyPaid = visit.BalanceDue <= 0;

        await _context.SaveChangesAsync();
    }



    private static string GetStatusIcon(VisitDisplayStatus status) => status switch
    {
        VisitDisplayStatus.NewNoResults => "\U0001F6D2",
        VisitDisplayStatus.ResultsNotWritten => "\U0001F6D2",
        VisitDisplayStatus.ResultsNotReviewed => "\U0001F3C5",
        VisitDisplayStatus.ResultsNotPrinted => "\U0001F4C4",
        VisitDisplayStatus.NotDelivered => "\U0001F5A8",
        VisitDisplayStatus.DeliveredWithBalance => "\U0001F4B2",
        VisitDisplayStatus.FullyComplete => "\u2705",
        _ => "\U0001F6D2"
    };

    private static string GetStatusColor(VisitDisplayStatus status) => status switch
    {
        VisitDisplayStatus.NewNoResults => "#808080",
        VisitDisplayStatus.ResultsNotWritten => "#FF8C00",
        VisitDisplayStatus.ResultsNotReviewed => "#FFD700",
        VisitDisplayStatus.ResultsNotPrinted => "#4FC3F7",
        VisitDisplayStatus.NotDelivered => "#9C27B0",
        VisitDisplayStatus.DeliveredWithBalance => "#F44336",
        VisitDisplayStatus.FullyComplete => "#4CAF50",
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

        // [VS-02] نقرأ BillingType للزيارة لتطبيق السعر المناسب على التحاليل المضافة.
        var billingType = await _context.Visits
            .AsNoTracking()
            .Where(v => v.VisitId == visitId)
            .Select(v => v.BillingType)
            .FirstOrDefaultAsync();

        foreach (var test in tests)
        {
            _context.VisitTests.Add(new VisitTest
            {
                VisitId = visitId,
                TesttypeId = test.TesttypeId,
                PriceCharged = ResolveBasePriceForBilling(test, billingType),
                CurrentStage = TestStage.Pending,
                IsOutsourced = false,
                AddedAt = DateTime.UtcNow
            });
        }
    }

    public async Task AddChargeToVisitAsync(int visitId, VisitCharge charge, int staffId)
    {
        charge.VisitId = visitId;
        charge.CreatedBy = staffId;
        charge.CreatedAt = DateTime.UtcNow;
        _context.VisitCharges.Add(charge);
        await _context.SaveChangesAsync();

        var visit = await _context.Visits.FindAsync(visitId);
        if (visit is not null)
        {
            var chargesTotal = await _context.VisitCharges
                .Where(c => c.VisitId == visitId)
                .SumAsync(c => c.Amount);
            visit.Subtotal += charge.Amount;
            visit.DiscountAmount = visit.Subtotal * visit.DiscountPercent / 100m;
            visit.TotalAfterDiscount = visit.Subtotal - visit.DiscountAmount;
            visit.BalanceDue = visit.TotalAfterDiscount - visit.TotalPaid;
            visit.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveChargeFromVisitAsync(int chargeId)
    {
        var charge = await _context.VisitCharges.FindAsync(chargeId);
        if (charge is null) return;

        var visitId = charge.VisitId;
        _context.VisitCharges.Remove(charge);
        await _context.SaveChangesAsync();

        var visit = await _context.Visits.FindAsync(visitId);
        if (visit is not null)
        {
            visit.Subtotal -= charge.Amount;
            visit.DiscountAmount = visit.Subtotal * visit.DiscountPercent / 100m;
            visit.TotalAfterDiscount = visit.Subtotal - visit.DiscountAmount;
            visit.BalanceDue = visit.TotalAfterDiscount - visit.TotalPaid;
            visit.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<VisitCharge>> GetVisitChargesAsync(int visitId)
    {
        return await _context.VisitCharges
            .Where(c => c.VisitId == visitId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// [VS-02 - القرار 12] يحدّد السعر الأساسي للتحليل وفق BillingType للزيارة.
    /// - Individual ⇒ PatientDefaultPrice.
    /// - LabToLab   ⇒ LabToLabDefaultPrice.
    /// - Free       ⇒ 0.
    /// تجاوز PriceScheme يتم تطبيقه في PricingService.GetPriceForTestAsync، لذلك يجب أن يبقى دور هذه الدالة محصورًا في الأساس.
    /// </summary>
    private static decimal ResolveBasePriceForBilling(TestType testType, BillingType billingType)
    {
        return billingType switch
        {
            BillingType.Individual => testType.PatientDefaultPrice,
            BillingType.LabToLab => testType.LabToLabDefaultPrice,
            BillingType.Free => 0m,
            _ => testType.PatientDefaultPrice
        };
    }
}
