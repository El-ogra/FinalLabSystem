using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class CommissionReportService : ICommissionReportService
{
    private readonly FinalLabDbContext _context;

    public CommissionReportService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommissionReportRow>> GetCommissionReportAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.VReferralCommissionReports
            .AsNoTracking()
            .Where(v => v.VisitDate >= startDate && v.VisitDate <= endDate)
            .Select(v => new CommissionReportRow
            {
                ReferralId = v.ReferralId,
                ReferralName = v.ReferralName,
                SourceType = v.SourceType,
                CommissionRate = v.CommissionRate,
                VisitId = v.VisitId,
                VisitCode = v.VisitCode,
                VisitDate = v.VisitDate,
                PatientName = v.PatientName,
                VisitTotal = v.VisitTotal,
                TotalPaid = v.TotalPaid,
                CommissionDue = v.CommissionDue
            })
            .ToListAsync();
    }
}
