using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class CashDrawerService : ICashDrawerService
{
    private const string PasswordKey = "CashDrawer.PasswordHash";

    private readonly FinalLabDbContext _context;
    private readonly ISettingsService _settingsService;
    private readonly ISensitiveScreenPasswordService _sensitivePasswordService;

    public CashDrawerService(
        FinalLabDbContext context,
        ISettingsService settingsService,
        ISensitiveScreenPasswordService sensitivePasswordService)
    {
        _context = context;
        _settingsService = settingsService;
        _sensitivePasswordService = sensitivePasswordService;
    }

    public async Task<CashDrawerSummaryDto> GetDailySummaryAsync(DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = date.ToDateTime(TimeOnly.MaxValue);

        var payments = await _context.Payments
            .Include(p => p.Visit)
                .ThenInclude(v => v!.Patient)
            .Where(p => p.PaymentDate >= start && p.PaymentDate <= end)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();

        return BuildSummary(date, payments);
    }

    public async Task<CashDrawerSummaryDto> GetSummaryByFilterAsync(CashDrawerFilterDto filter)
    {
        var query = _context.Payments
            .Include(p => p.Visit)
                .ThenInclude(v => v!.Patient)
            .AsQueryable();

        if (filter.FromDate.HasValue)
        {
            var start = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(p => p.PaymentDate >= start);
        }

        if (filter.ToDate.HasValue)
        {
            var end = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(p => p.PaymentDate <= end);
        }

        if (filter.StaffId.HasValue)
            query = query.Where(p => p.ReceivedBy == filter.StaffId.Value);

        var payments = await query.OrderBy(p => p.PaymentDate).ToListAsync();

        var dateLabel = filter.FromDate == filter.ToDate && filter.FromDate.HasValue
            ? filter.FromDate.Value
            : DateOnly.FromDateTime(DateTime.Today);

        return BuildSummary(dateLabel, payments);
    }

    public async Task<bool> IsPasswordSetAsync()
    {
        return await _sensitivePasswordService.IsPasswordSetAsync("CashDrawer");
    }

    public async Task<bool> UnlockAsync(string password)
    {
        return await _sensitivePasswordService.VerifyAsync("CashDrawer", password);
    }

    public async Task SetPasswordAsync(string newPassword)
    {
        await _sensitivePasswordService.SetPasswordAsync("CashDrawer", newPassword, 0);
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var isSet = await _sensitivePasswordService.IsPasswordSetAsync("CashDrawer");
        if (!isSet)
            throw new InvalidOperationException("لم تُعد كلمة مرور لدرج النقدية بعد.");
        await _sensitivePasswordService.ChangePasswordAsync("CashDrawer", currentPassword, newPassword, 0);
    }

    private static CashDrawerSummaryDto BuildSummary(DateOnly date, List<Payment> payments)
    {
        // [القرار 12 - VS-01] تحديث حسابات درج النقدية وفق القيم الجديدة لـ PaymentMethod.
        var cashTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
        var cardTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Card).Sum(p => p.Amount);
        var checkTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Check).Sum(p => p.Amount);
        var insuranceTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Insurance).Sum(p => p.Amount);
        var otherTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Other).Sum(p => p.Amount);

        return new CashDrawerSummaryDto
        {
            Date = date,
            TotalCashReceived = cashTotal,
            TotalCardReceived = cardTotal,
            TotalCheckReceived = checkTotal,
            TotalInsuranceReceived = insuranceTotal,
            TotalOtherReceived = otherTotal,
            GrandTotal = cashTotal + cardTotal + checkTotal + insuranceTotal + otherTotal,
            PaymentCount = payments.Count,
            Payments = payments.Select(p => new CashDrawerPaymentRow
            {
                PaymentId = p.PaymentId,
                PatientName = p.Visit?.Patient?.FullNameAr ?? "—",
                VisitCode = p.Visit?.VisitCode ?? "—",
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                PaymentDate = p.PaymentDate
            }).ToList()
        };
    }
}
