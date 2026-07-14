using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class FinancialServiceDiscountLimitTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static FinancialService CreateService(FinalLabDbContext ctx, Mock<IAuditService>? auditMock = null)
    {
        auditMock ??= new Mock<IAuditService>();
        var visitServiceMock = new Mock<IVisitService>();
        var logger = new Mock<ILogger<FinancialService>>();
        return new FinancialService(ctx, logger.Object, visitServiceMock.Object, auditMock.Object);
    }

    [Fact]
    public async Task ApplyDiscountAsync_WhenDiscountExceedsLimit_RejectsAndLogsAudit()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ApplyDiscountAsync_WhenDiscountExceedsLimit_RejectsAndLogsAudit)));

        var staff = new Staff
        {
            Username = "testuser",
            DisplayName = "Test User",
            PasswordHash = "hash",
            IsAdmin = false,
            IsActive = true,
            DiscountLimit = 10.0
        };
        ctx.Staff.Add(staff);

        var visit = new Visit
        {
            VisitCode = "V001",
            PatientId = 1,
            VisitDate = DateTime.UtcNow,
            Subtotal = 1000m,
            DiscountAmount = 0,
            DiscountPercent = 0,
            TotalAfterDiscount = 1000m,
            BalanceDue = 1000m,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        var auditMock = new Mock<IAuditService>();
        var service = CreateService(ctx, auditMock);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApplyDiscountAsync(visit.VisitId, 15m, staff.StaffId));

        Assert.Contains("الخصم", ex.Message);
        Assert.Contains("15", ex.Message);
        Assert.Contains("10", ex.Message);

        auditMock.Verify(a => a.LogActionAsync(
            "Visit",
            visit.VisitId,
            "DISCOUNT_REJECTED",
            staff.StaffId,
            It.Is<string>(s => s.Contains("15") && s.Contains("10"))),
            Times.Once);
    }

    [Fact]
    public async Task ApplyDiscountAsync_WhenDiscountWithinLimit_AppliesSuccessfully()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ApplyDiscountAsync_WhenDiscountWithinLimit_AppliesSuccessfully)));

        var staff = new Staff
        {
            Username = "testuser2",
            DisplayName = "Test User 2",
            PasswordHash = "hash",
            IsAdmin = false,
            IsActive = true,
            DiscountLimit = 20.0
        };
        ctx.Staff.Add(staff);

        var visit = new Visit
        {
            VisitCode = "V002",
            PatientId = 2,
            VisitDate = DateTime.UtcNow,
            Subtotal = 500m,
            DiscountAmount = 0,
            DiscountPercent = 0,
            TotalAfterDiscount = 500m,
            BalanceDue = 500m,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        var auditMock = new Mock<IAuditService>();
        var service = CreateService(ctx, auditMock);

        await service.ApplyDiscountAsync(visit.VisitId, 15m, staff.StaffId);

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.Equal(15m, updatedVisit.DiscountPercent);
        Assert.Equal(75m, updatedVisit.DiscountAmount);
        Assert.Equal(425m, updatedVisit.TotalAfterDiscount);

        auditMock.Verify(a => a.LogActionAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            "DISCOUNT_REJECTED",
            It.IsAny<int>(),
            It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyDiscountAsync_AdminIsExemptFromLimit()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ApplyDiscountAsync_AdminIsExemptFromLimit)));

        var adminStaff = new Staff
        {
            Username = "admin",
            DisplayName = "Admin User",
            PasswordHash = "hash",
            IsAdmin = true,
            IsActive = true,
            DiscountLimit = 5.0
        };
        ctx.Staff.Add(adminStaff);

        var visit = new Visit
        {
            VisitCode = "V003",
            PatientId = 3,
            VisitDate = DateTime.UtcNow,
            Subtotal = 200m,
            DiscountAmount = 0,
            DiscountPercent = 0,
            TotalAfterDiscount = 200m,
            BalanceDue = 200m,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        var auditMock = new Mock<IAuditService>();
        var service = CreateService(ctx, auditMock);

        // Admin can apply 50% discount even though limit is 5%
        await service.ApplyDiscountAsync(visit.VisitId, 50m, adminStaff.StaffId);

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.Equal(50m, updatedVisit.DiscountPercent);

        auditMock.Verify(a => a.LogActionAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            "DISCOUNT_REJECTED",
            It.IsAny<int>(),
            It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyDiscountAsync_ThrowsWhenStaffNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ApplyDiscountAsync_ThrowsWhenStaffNotFound)));

        var visit = new Visit
        {
            VisitCode = "V004",
            PatientId = 4,
            VisitDate = DateTime.UtcNow,
            Subtotal = 100m,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApplyDiscountAsync(visit.VisitId, 10m, 9999));
    }

    [Fact]
    public async Task ApplyDiscountAsync_WhenDiscountEqualsLimit_AppliesSuccessfully()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ApplyDiscountAsync_WhenDiscountEqualsLimit_AppliesSuccessfully)));

        var staff = new Staff
        {
            Username = "testuser3",
            DisplayName = "Test User 3",
            PasswordHash = "hash",
            IsAdmin = false,
            IsActive = true,
            DiscountLimit = 15.0
        };
        ctx.Staff.Add(staff);

        var visit = new Visit
        {
            VisitCode = "V005",
            PatientId = 5,
            VisitDate = DateTime.UtcNow,
            Subtotal = 300m,
            DiscountAmount = 0,
            DiscountPercent = 0,
            TotalAfterDiscount = 300m,
            BalanceDue = 300m,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);

        // Discount exactly equal to limit should be accepted
        await service.ApplyDiscountAsync(visit.VisitId, 15m, staff.StaffId);

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.Equal(15m, updatedVisit.DiscountPercent);
    }
}
