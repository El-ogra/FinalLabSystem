using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class CashDrawerServiceTests : IDisposable
{
    private readonly FinalLabDbContext _context;
    private readonly Mock<ISettingsService> _settingsMock;
    private readonly CashDrawerService _service;

    public CashDrawerServiceTests()
    {
        var options = new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase($"CashDrawerTests_{Guid.NewGuid()}")
            .Options;
        _context = new FinalLabDbContext(options);
        _settingsMock = new Mock<ISettingsService>();
        _service = new CashDrawerService(_context, _settingsMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private async Task SeedPaymentsAsync()
    {
        var patient = new Patient { PatientId = 1, FullNameAr = "أحمد", PatientCode = "P001", Sex = "M" };
        _context.Patients.Add(patient);

        var staff = new Staff { StaffId = 1, DisplayName = "موظف", Username = "testuser", PasswordHash = "hash", IsActive = true };
        _context.Staff.Add(staff);

        var visit = new Visit
        {
            VisitId = 1,
            VisitCode = "V001",
            PatientId = 1,
            VisitDate = DateTime.Today,
            Subtotal = 100,
            TotalPaid = 100,
            PaymentStatus = PaymentStatus.Paid
        };
        _context.Visits.Add(visit);

        _context.Payments.AddRange(
            new Payment { PaymentId = 1, VisitId = 1, Amount = 50, PaymentMethod = PaymentMethod.Cash, PaymentType = "Full", ReceivedBy = 1, PaymentDate = DateTime.Today.AddHours(10) },
            new Payment { PaymentId = 2, VisitId = 1, Amount = 30, PaymentMethod = PaymentMethod.Insurance, PaymentType = "Full", ReceivedBy = 1, PaymentDate = DateTime.Today.AddHours(11) },
            new Payment { PaymentId = 3, VisitId = 1, Amount = 20, PaymentMethod = PaymentMethod.Contract, PaymentType = "Full", ReceivedBy = 1, PaymentDate = DateTime.Today.AddHours(12) }
        );

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDailySummaryAsync_ReturnsCorrectTotals()
    {
        await SeedPaymentsAsync();
        var date = DateOnly.FromDateTime(DateTime.Today);

        var result = await _service.GetDailySummaryAsync(date);

        Assert.Equal(50, result.TotalCashReceived);
        Assert.Equal(30, result.TotalInsuranceReceived);
        Assert.Equal(20, result.TotalContractReceived);
        Assert.Equal(100, result.GrandTotal);
        Assert.Equal(3, result.PaymentCount);
    }

    [Fact]
    public async Task GetDailySummaryAsync_NoPayments_ReturnsZeros()
    {
        var date = DateOnly.FromDateTime(DateTime.Today);

        var result = await _service.GetDailySummaryAsync(date);

        Assert.Equal(0, result.TotalCashReceived);
        Assert.Equal(0, result.GrandTotal);
        Assert.Empty(result.Payments);
    }

    [Fact]
    public async Task GetSummaryByFilterAsync_WithDateRange_ReturnsFiltered()
    {
        await SeedPaymentsAsync();

        var filter = new CashDrawerFilterDto
        {
            FromDate = DateOnly.FromDateTime(DateTime.Today),
            ToDate = DateOnly.FromDateTime(DateTime.Today)
        };

        var result = await _service.GetSummaryByFilterAsync(filter);

        Assert.Equal(3, result.PaymentCount);
    }

    [Fact]
    public async Task GetSummaryByFilterAsync_WithStaffFilter_ReturnsFiltered()
    {
        await SeedPaymentsAsync();

        var filter = new CashDrawerFilterDto { StaffId = 1 };

        var result = await _service.GetSummaryByFilterAsync(filter);

        Assert.Equal(3, result.PaymentCount);
    }

    [Fact]
    public async Task IsPasswordSetAsync_NoPassword_ReturnsFalse()
    {
        _settingsMock.Setup(s => s.GetSettingValueAsync("CashDrawer.PasswordHash"))
            .ReturnsAsync((string?)null);

        var result = await _service.IsPasswordSetAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task IsPasswordSetAsync_HasPassword_ReturnsTrue()
    {
        _settingsMock.Setup(s => s.GetSettingValueAsync("CashDrawer.PasswordHash"))
            .ReturnsAsync("pbkdf2$100000$abc$def");

        var result = await _service.IsPasswordSetAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task UnlockAsync_CorrectPassword_ReturnsTrue()
    {
        var hash = FinalLabSystem.Infrastructure.Security.PasswordHasher.Hash("test123");
        _settingsMock.Setup(s => s.GetSettingValueAsync("CashDrawer.PasswordHash"))
            .ReturnsAsync(hash);

        var result = await _service.UnlockAsync("test123");

        Assert.True(result);
    }

    [Fact]
    public async Task UnlockAsync_WrongPassword_ReturnsFalse()
    {
        var hash = FinalLabSystem.Infrastructure.Security.PasswordHasher.Hash("test123");
        _settingsMock.Setup(s => s.GetSettingValueAsync("CashDrawer.PasswordHash"))
            .ReturnsAsync(hash);

        var result = await _service.UnlockAsync("wrong");

        Assert.False(result);
    }

    [Fact]
    public async Task SetPasswordAsync_SavesHashedPassword()
    {
        _settingsMock.Setup(s => s.UpsertSettingAsync(It.IsAny<LabSetting>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        await _service.SetPasswordAsync("newpass");

        _settingsMock.Verify(s => s.UpsertSettingAsync(
            It.Is<LabSetting>(ls => ls.SettingKey == "CashDrawer.PasswordHash" && !string.IsNullOrEmpty(ls.SettingValue)),
            It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_CorrectCurrent_ChangesSuccessfully()
    {
        var hash = FinalLabSystem.Infrastructure.Security.PasswordHasher.Hash("oldpass");
        _settingsMock.Setup(s => s.GetSettingValueAsync("CashDrawer.PasswordHash"))
            .ReturnsAsync(hash);
        _settingsMock.Setup(s => s.UpsertSettingAsync(It.IsAny<LabSetting>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        await _service.ChangePasswordAsync("oldpass", "newpass");

        _settingsMock.Verify(s => s.UpsertSettingAsync(It.IsAny<LabSetting>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrent_ThrowsUnauthorized()
    {
        var hash = FinalLabSystem.Infrastructure.Security.PasswordHasher.Hash("oldpass");
        _settingsMock.Setup(s => s.GetSettingValueAsync("CashDrawer.PasswordHash"))
            .ReturnsAsync(hash);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.ChangePasswordAsync("wrong", "newpass"));
    }

    [Fact]
    public async Task ChangePasswordAsync_NoPasswordSet_ThrowsInvalidOperation()
    {
        _settingsMock.Setup(s => s.GetSettingValueAsync("CashDrawer.PasswordHash"))
            .ReturnsAsync((string?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ChangePasswordAsync("old", "new"));
    }
}
