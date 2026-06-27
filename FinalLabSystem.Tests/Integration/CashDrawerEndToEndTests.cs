using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace FinalLabSystem.Tests.Integration;

public class CashDrawerEndToEndTests
{
    private class InMemorySettingsService : ISettingsService
    {
        private readonly Dictionary<string, string> _store = new();

        public Task<string?> GetSettingValueAsync(string key)
        {
            _store.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public Task UpsertSettingAsync(LabSetting setting, int staffId)
        {
            _store[setting.SettingKey] = setting.SettingValue ?? string.Empty;
            return Task.CompletedTask;
        }

        public Task<Dictionary<string, string>> GetSettingsByGroupAsync(string groupName)
        {
            return Task.FromResult(_store);
        }
    }

    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static (CashDrawerService service, InMemorySettingsService settings) CreateService(FinalLabDbContext ctx)
    {
        var settings = new InMemorySettingsService();
        var service = new CashDrawerService(ctx, settings);
        return (service, settings);
    }

    [Fact]
    public async Task EndToEnd_SetPasswordAndUnlock_Success()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_SetPasswordAndUnlock_Success)));
        var (service, _) = CreateService(ctx);

        var isSetBefore = await service.IsPasswordSetAsync();
        Assert.False(isSetBefore);

        await service.SetPasswordAsync("drawer123");

        var isSetAfter = await service.IsPasswordSetAsync();
        Assert.True(isSetAfter);

        var unlockResult = await service.UnlockAsync("drawer123");
        Assert.True(unlockResult);
    }

    [Fact]
    public async Task EndToEnd_UnlockWithWrongPassword_Fails()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_UnlockWithWrongPassword_Fails)));
        var (service, _) = CreateService(ctx);

        await service.SetPasswordAsync("correct_password");

        var wrongResult = await service.UnlockAsync("wrong_password");
        Assert.False(wrongResult);
    }

    [Fact]
    public async Task EndToEnd_GetDailySummary_ReturnsData()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_GetDailySummary_ReturnsData)));

        var staff = new Staff { StaffId = 1, DisplayName = "Cashier", IsActive = true, Username = "cashier1", PasswordHash = "hash" };
        ctx.Staff.Add(staff);

        var patient = new Patient
        {
            PatientCode = "P100",
            FullNameAr = "مريض تجريبي",
            FullNameEn = "Test Patient",
            Sex = "M"
        };
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var visit = new Visit
        {
            VisitCode = "V200",
            PatientId = patient.PatientId,
            VisitDate = DateTime.UtcNow,
            TotalAfterDiscount = 500
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        ctx.Payments.AddRange(
            new Payment
            {
                VisitId = visit.VisitId,
                Amount = 300,
                PaymentMethod = Models.Enums.PaymentMethod.Cash,
                PaymentType = "PAYMENT",
                PaymentDate = DateTime.UtcNow,
                ReceivedBy = 1
            },
            new Payment
            {
                VisitId = visit.VisitId,
                Amount = 200,
                PaymentMethod = Models.Enums.PaymentMethod.Insurance,
                PaymentType = "PAYMENT",
                PaymentDate = DateTime.UtcNow,
                ReceivedBy = 1
            });
        await ctx.SaveChangesAsync();

        var (service, _) = CreateService(ctx);
        var summary = await service.GetDailySummaryAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.Equal(300, summary.TotalCashReceived);
        Assert.Equal(200, summary.TotalInsuranceReceived);
        Assert.Equal(500, summary.GrandTotal);
        Assert.Equal(2, summary.PaymentCount);
    }

    [Fact]
    public async Task EndToEnd_ChangePassword_Success()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_ChangePassword_Success)));
        var (service, _) = CreateService(ctx);

        await service.SetPasswordAsync("old_pass");

        var oldUnlock = await service.UnlockAsync("old_pass");
        Assert.True(oldUnlock);

        await service.ChangePasswordAsync("old_pass", "new_pass");

        var newUnlock = await service.UnlockAsync("new_pass");
        Assert.True(newUnlock);

        var oldStillWorks = await service.UnlockAsync("old_pass");
        Assert.False(oldStillWorks);
    }

    [Fact]
    public async Task EndToEnd_IsPasswordSet_ReturnsCorrectState()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_IsPasswordSet_ReturnsCorrectState)));
        var (service, _) = CreateService(ctx);

        var beforeSet = await service.IsPasswordSetAsync();
        Assert.False(beforeSet);

        await service.SetPasswordAsync("test_pwd");

        var afterSet = await service.IsPasswordSetAsync();
        Assert.True(afterSet);
    }
}
