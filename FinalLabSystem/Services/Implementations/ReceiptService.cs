using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public sealed class ReceiptService : IReceiptService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(FinalLabDbContext context, ILogger<ReceiptService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> CanPrintReceiptAsync(int visitId, int staffId)
    {
        var staff = await _context.Staff.FindAsync(staffId);
        if (staff?.IsAdmin == true)
            return true;

        var visit = await _context.Visits.FindAsync(visitId);
        if (visit is null)
            return false;

        var lastLog = await GetLastPrintLogAsync(visitId);
        if (lastLog is null)
            return true;

        return lastLog.Subtotal != visit.Subtotal
            || lastLog.DiscountAmount != visit.DiscountAmount
            || lastLog.TotalAfterDiscount != visit.TotalAfterDiscount
            || lastLog.TotalPaid != visit.TotalPaid
            || lastLog.BalanceDue != visit.BalanceDue;
    }

    public async Task LogPrintEventAsync(ReceiptPrintLog logEntry)
    {
        _context.ReceiptPrintLogs.Add(logEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<ReceiptPrintLog?> GetLastPrintLogAsync(int visitId)
    {
        return await _context.ReceiptPrintLogs
            .Where(l => l.VisitId == visitId)
            .OrderByDescending(l => l.PrintedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ReceiptGroupedTest>> GetGroupedTestsForReceiptAsync(int visitId)
    {
        var visitTests = await _context.VisitTests
            .Where(vt => vt.VisitId == visitId)
            .Include(vt => vt.Testtype)
                .ThenInclude(t => t.Group)
            .OrderBy(vt => vt.VisitTestId)
            .ToListAsync();

        var selectedGroupIds = visitTests
            .Where(vt => vt.Testtype?.Group is not null)
            .Select(vt => vt.Testtype.GroupId)
            .Distinct()
            .ToList();

        var groupTestCounts = await _context.TestTypes
            .Where(t => selectedGroupIds.Contains(t.GroupId) && t.IsActive)
            .GroupBy(t => t.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.GroupId, g => g.Count);

        var result = new List<ReceiptGroupedTest>();

        var groupedTests = visitTests
            .Where(vt => vt.Testtype?.Group is not null)
            .GroupBy(vt => vt.Testtype.GroupId);

        foreach (var group in groupedTests)
        {
            var groupId = group.Key;
            var testType = group.First().Testtype;
            var groupName = testType.Group?.GroupNameAr ?? testType.Group?.GroupNameEn ?? "Unknown";

            var totalCountInGroup = groupTestCounts.TryGetValue(groupId, out var count) ? count : group.Count();
            var selectedCount = group.Count();
            var isSummarized = selectedCount >= totalCountInGroup && totalCountInGroup > 1;

            if (isSummarized)
            {
                var testNames = group.Select(vt => vt.Testtype.TypeNameAr ?? vt.Testtype.TypeNameEn)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                result.Add(new ReceiptGroupedTest
                {
                    GroupName = groupName,
                    TestCount = totalCountInGroup,
                    TotalPrice = group.Sum(vt => vt.PriceCharged),
                    IsSummarized = true,
                    DetailLine = string.Join(", ", testNames)
                });
            }
            else
            {
                foreach (var vt in group.OrderBy(v => v.VisitTestId))
                {
                    result.Add(new ReceiptGroupedTest
                    {
                        GroupName = vt.Testtype.TypeNameAr ?? vt.Testtype.TypeNameEn ?? "Unknown",
                        TestCount = 1,
                        TotalPrice = vt.PriceCharged,
                        IsSummarized = false,
                        DetailLine = null
                    });
                }
            }
        }

        var noGroupTests = visitTests.Where(vt => vt.Testtype?.Group is null);
        foreach (var vt in noGroupTests)
        {
            result.Add(new ReceiptGroupedTest
            {
                GroupName = vt.Testtype.TypeNameAr ?? vt.Testtype.TypeNameEn ?? "Unknown",
                TestCount = 1,
                TotalPrice = vt.PriceCharged,
                IsSummarized = false,
                DetailLine = null
            });
        }

        return result;
    }
}
