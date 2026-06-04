using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class VisitService : IVisitService
{
    private readonly FinalLabDbContext _context;

    public VisitService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            visit.VisitStatus = "OPEN";
            visit.PaymentStatus = "PENDING";
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
                        CurrentStage = "PENDING",
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

    public async Task CancelVisitTestAsync(int visitTestId)
    {
        var visitTest = await _context.VisitTests.FindAsync(visitTestId);
        if (visitTest != null)
        {
            visitTest.CurrentStage = "CANCELLED";
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Visit?> GetVisitSummaryAsync(int visitId)
    {
        return await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.VisitTests)
                .ThenInclude(vt => vt.Testtype)
            .Include(v => v.VisitCharges)
            .Include(v => v.Scheme)
            .Include(v => v.Receptionist)
            .FirstOrDefaultAsync(v => v.VisitId == visitId);
    }
}
