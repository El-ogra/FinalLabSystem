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

    public CashDrawerService(FinalLabDbContext context, ISettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
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
        var hash = await _settingsService.GetSettingValueAsync(PasswordKey);
        return !string.IsNullOrEmpty(hash);
    }

    public async Task<bool> UnlockAsync(string password)
    {
        var hash = await _settingsService.GetSettingValueAsync(PasswordKey);
        if (string.IsNullOrEmpty(hash))
            return false;

        return PasswordHasher.Verify(password, hash);
    }

    public async Task SetPasswordAsync(string newPassword)
    {
        var hash = PasswordHasher.Hash(newPassword);
        var setting = new LabSetting
        {
            SettingKey = PasswordKey,
            SettingValue = hash,
            SettingDescription = "Cash drawer password hash",
            SettingGroup = "CashDrawer",
            IsRequired = true
        };
        await _settingsService.UpsertSettingAsync(setting, 0);
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var currentHash = await _settingsService.GetSettingValueAsync(PasswordKey);
        if (string.IsNullOrEmpty(currentHash))
            throw new InvalidOperationException("لم تُعد كلمة مرور لدرج النقدية بعد.");

        if (!PasswordHasher.Verify(currentPassword, currentHash))
            throw new UnauthorizedAccessException("كلمة المرور الحالية غير صحيحة.");

        await SetPasswordAsync(newPassword);
    }

    private static CashDrawerSummaryDto BuildSummary(DateOnly date, List<Payment> payments)
    {
        var cashTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);
        var insuranceTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Insurance).Sum(p => p.Amount);
        var contractTotal = payments.Where(p => p.PaymentMethod == PaymentMethod.Contract).Sum(p => p.Amount);

        return new CashDrawerSummaryDto
        {
            Date = date,
            TotalCashReceived = cashTotal,
            TotalInsuranceReceived = insuranceTotal,
            TotalContractReceived = contractTotal,
            GrandTotal = cashTotal + insuranceTotal + contractTotal,
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
