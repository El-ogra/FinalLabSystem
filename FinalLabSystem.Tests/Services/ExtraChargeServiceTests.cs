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

public class ExtraChargeServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static VisitService CreateVisitService(FinalLabDbContext ctx)
    {
        var logger = new Mock<ILogger<VisitService>>();
        return new VisitService(ctx, logger.Object);
    }

    [Fact]
    public async Task AddChargeToVisitAsync_AddsChargeAndUpdatesSubtotal()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(AddChargeToVisitAsync_AddsChargeAndUpdatesSubtotal)));

        var visit = new Visit
        {
            VisitCode = "V001",
            PatientId = 1,
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

        var service = CreateVisitService(ctx);
        var charge = new VisitCharge
        {
            ChargeDescription = "خدمة السحب المنزلي",
            Amount = 50m,
            ChargeType = "HomeCollection"
        };

        await service.AddChargeToVisitAsync(visit.VisitId, charge, 1);

        var charges = await ctx.VisitCharges.Where(c => c.VisitId == visit.VisitId).ToListAsync();
        Assert.Single(charges);
        Assert.Equal(50m, charges[0].Amount);
        Assert.Equal("خدمة السحب المنزلي", charges[0].ChargeDescription);

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.Equal(550m, updatedVisit.Subtotal);
    }

    [Fact]
    public async Task AddMultipleCharges_IncreasesSubtotalCorrectly()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(AddMultipleCharges_IncreasesSubtotalCorrectly)));

        var visit = new Visit
        {
            VisitCode = "V002",
            PatientId = 2,
            VisitDate = DateTime.UtcNow,
            Subtotal = 300m,
            DiscountAmount = 30m,
            DiscountPercent = 10,
            TotalAfterDiscount = 270m,
            BalanceDue = 270m,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        var service = CreateVisitService(ctx);

        var charge1 = new VisitCharge { ChargeDescription = "رسوم الاستعجال", Amount = 25m, ChargeType = "RushFee" };
        var charge2 = new VisitCharge { ChargeDescription = "خدمة السحب المنزلي", Amount = 50m, ChargeType = "HomeCollection" };

        await service.AddChargeToVisitAsync(visit.VisitId, charge1, 1);
        await service.AddChargeToVisitAsync(visit.VisitId, charge2, 1);

        var charges = await ctx.VisitCharges.Where(c => c.VisitId == visit.VisitId).ToListAsync();
        Assert.Equal(2, charges.Count);
        Assert.Equal(75m, charges.Sum(c => c.Amount));

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.Equal(375m, updatedVisit.Subtotal);
    }

    [Fact]
    public async Task RemoveChargeFromVisitAsync_RemovesChargeAndUpdatesSubtotal()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(RemoveChargeFromVisitAsync_RemovesChargeAndUpdatesSubtotal)));

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

            var charge = new VisitCharge
            {
                VisitId = visit.VisitId,
                ChargeDescription = "رسوم إضافية",
                Amount = 30m,
                ChargeType = "Other",
                CreatedAt = DateTime.UtcNow
            };
        ctx.VisitCharges.Add(charge);
        await ctx.SaveChangesAsync();

        // Manually update visit subtotal as if charge was added
        visit.Subtotal = 230m;
        visit.TotalAfterDiscount = 230m;
        visit.BalanceDue = 230m;
        await ctx.SaveChangesAsync();

        var service = CreateVisitService(ctx);
        await service.RemoveChargeFromVisitAsync(charge.ChargeId);

        var remainingCharges = await ctx.VisitCharges.Where(c => c.VisitId == visit.VisitId).ToListAsync();
        Assert.Empty(remainingCharges);

        var updatedVisit = await ctx.Visits.FindAsync(visit.VisitId);
        Assert.NotNull(updatedVisit);
        Assert.Equal(200m, updatedVisit.Subtotal);
    }

    [Fact]
    public async Task GetVisitChargesAsync_ReturnsChargesOrderedByDate()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetVisitChargesAsync_ReturnsChargesOrderedByDate)));

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

        ctx.VisitCharges.Add(new VisitCharge { VisitId = visit.VisitId, ChargeDescription = "First", Amount = 10m, CreatedAt = DateTime.UtcNow.AddMinutes(-5) });
        ctx.VisitCharges.Add(new VisitCharge { VisitId = visit.VisitId, ChargeDescription = "Second", Amount = 20m, CreatedAt = DateTime.UtcNow });
        await ctx.SaveChangesAsync();

        var service = CreateVisitService(ctx);
        var charges = await service.GetVisitChargesAsync(visit.VisitId);

        Assert.Equal(2, charges.Count);
        Assert.Equal("First", charges[0].ChargeDescription);
        Assert.Equal("Second", charges[1].ChargeDescription);
    }
}
