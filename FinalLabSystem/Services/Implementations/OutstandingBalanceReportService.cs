using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class OutstandingBalanceReportService : IOutstandingBalanceReportService
{
    private readonly FinalLabDbContext _context;

    public OutstandingBalanceReportService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<OutstandingBalanceReportRow>> GetOutstandingBalancesAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.VOutstandingBalances
            .AsNoTracking()
            .Where(v => v.VisitDate >= startDate && v.VisitDate <= endDate)
            .Select(v => new OutstandingBalanceReportRow
            {
                VisitId = v.VisitId,
                VisitCode = v.VisitCode,
                VisitDate = v.VisitDate,
                PatientCode = v.PatientCode,
                PatientName = v.PatientName,
                Phone = v.Phone,
                CompanyName = v.CompanyName,
                TotalAfterDiscount = v.TotalAfterDiscount,
                TotalPaid = v.TotalPaid,
                BalanceDue = v.BalanceDue,
                PaymentStatus = v.PaymentStatus,
                DaysOverdue = v.DaysOverdue
            })
            .ToListAsync();
    }
}
