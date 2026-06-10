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

public class ReportingService : IReportingService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<ReportingService> _logger;

    public ReportingService(FinalLabDbContext context, ILogger<ReportingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<VPendingTest>> GetPendingWorksheetsAsync()
    {
        return await _context.VPendingTests
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<VOutstandingBalance>> GetDefaultersListAsync()
    {
        return await _context.VOutstandingBalances
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<VReferralCommissionReport>> GetCommissionsAsync(DateTime start, DateTime end)
    {
        return await _context.VReferralCommissionReports
            .AsNoTracking()
            .Where(r => r.VisitDate >= start && r.VisitDate <= end)
            .ToListAsync();
    }

    public async Task<List<VPatientHistory>> GetHistoricalComparisonsAsync(int patientId, int testTypeId)
    {
        return await _context.VPatientHistories
            .AsNoTracking()
            .Where(vh => vh.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<object> GetDashboardMetricsAsync(DateTime date)
    {
        var todayStart = date.Date;
        var todayEnd = date.Date.AddDays(1);

        var pendingTests = await _context.VPendingTests.AsNoTracking().CountAsync();
        var todayVisits = await _context.Visits
            .AsNoTracking()
            .CountAsync(v => v.VisitDate >= todayStart && v.VisitDate < todayEnd);
        var outstandingBalance = await _context.VOutstandingBalances
            .AsNoTracking()
            .SumAsync(b => b.BalanceDue);
        var todayPayments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.PaymentDate >= todayStart && p.PaymentDate < todayEnd)
            .SumAsync(p => p.Amount);

        return new Dictionary<string, object>
        {
            ["PendingTests"] = pendingTests,
            ["TodayVisits"] = todayVisits,
            ["OutstandingBalance"] = outstandingBalance,
            ["TodayPayments"] = todayPayments
        };
    }
}
