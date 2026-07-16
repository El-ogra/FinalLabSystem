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

public class InvoiceFlowCalculateBeforeSaveTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    [Fact]
    public async Task SavePatientVisitAsync_WithExtraCharges_SavesChargesAndUpdatesSubtotal()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(SavePatientVisitAsync_WithExtraCharges_SavesChargesAndUpdatesSubtotal)));

        var testType = new TestType
        {
            TypeCode = "CBC",
            TypeNameEn = "CBC",
            DefaultPrice = 100m,
            PatientDefaultPrice = 100m,
            LabToLabDefaultPrice = 70m,
            SampleType = "Blood",
            TurnaroundHours = 24
        };
        ctx.TestTypes.Add(testType);
        await ctx.SaveChangesAsync();

        var logger = new Mock<ILogger<VisitService>>();
        var authService = new Mock<IAuthService>();
        var visitService = new VisitService(ctx, logger.Object, authService.Object);

        var patient = new Patient
        {
            FullNameAr = "مريض تجريبي",
            Sex = "M",
            PatientCode = "P001",
            CreatedAt = DateTime.UtcNow
        };

        var visit = new Visit
        {
            VisitCode = "V001",
            VisitDate = DateTime.UtcNow,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var charges = new List<VisitCharge>
        {
            new() { ChargeDescription = "خدمة السحب المنزلي", Amount = 50m, ChargeType = "HomeCollection" },
            new() { ChargeDescription = "رسوم الاستعجال", Amount = 25m, ChargeType = "RushFee" }
        };

        var saved = await visitService.SavePatientVisitAsync(
            patient, visit, new List<int> { testType.TesttypeId }, 0, 1,
            new List<PatientMedicalHistory>(), null, charges);

        Assert.NotNull(saved);
        var savedCharges = await ctx.VisitCharges
            .Where(c => c.VisitId == saved.VisitId)
            .ToListAsync();
        Assert.Equal(2, savedCharges.Count);
        Assert.Equal(75m, savedCharges.Sum(c => c.Amount));
        Assert.Equal(175m, saved.Subtotal); // 100 (test) + 75 (charges)
    }

    [Fact]
    public async Task SavePatientVisitAsync_WithoutExtraCharges_SavesNormally()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(SavePatientVisitAsync_WithoutExtraCharges_SavesNormally)));

        var testType = new TestType
        {
            TypeCode = "CBC2",
            TypeNameEn = "CBC2",
            DefaultPrice = 200m,
            PatientDefaultPrice = 200m,
            LabToLabDefaultPrice = 140m,
            SampleType = "Blood",
            TurnaroundHours = 24
        };
        ctx.TestTypes.Add(testType);
        await ctx.SaveChangesAsync();

        var logger = new Mock<ILogger<VisitService>>();
        var authService = new Mock<IAuthService>();
        var visitService = new VisitService(ctx, logger.Object, authService.Object);

        var patient = new Patient
        {
            FullNameAr = "مريض تجريبي 2",
            Sex = "F",
            PatientCode = "P002",
            CreatedAt = DateTime.UtcNow
        };

        var visit = new Visit
        {
            VisitCode = "V002",
            VisitDate = DateTime.UtcNow,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var saved = await visitService.SavePatientVisitAsync(
            patient, visit, new List<int> { testType.TesttypeId }, 0, 1,
            new List<PatientMedicalHistory>(), null, null);

        Assert.NotNull(saved);
        Assert.Equal(200m, saved.Subtotal);
        Assert.Empty(await ctx.VisitCharges.Where(c => c.VisitId == saved.VisitId).ToListAsync());
    }

    [Fact]
    public async Task VisitService_GetVisitChargesAsync_ReturnsOrderedCharges()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(VisitService_GetVisitChargesAsync_ReturnsOrderedCharges)));

        var visit = new Visit
        {
            VisitCode = "V003",
            PatientId = 3,
            VisitDate = DateTime.UtcNow,
            Subtotal = 100m,
            BillingType = BillingType.Individual,
            PaymentStatus = PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        await ctx.SaveChangesAsync();

        ctx.VisitCharges.Add(new VisitCharge { VisitId = visit.VisitId, ChargeDescription = "Second", Amount = 20m, CreatedAt = DateTime.UtcNow });
        ctx.VisitCharges.Add(new VisitCharge { VisitId = visit.VisitId, ChargeDescription = "First", Amount = 10m, CreatedAt = DateTime.UtcNow.AddMinutes(-10) });
        await ctx.SaveChangesAsync();

        var logger = new Mock<ILogger<VisitService>>();
        var authService = new Mock<IAuthService>();
        var visitService = new VisitService(ctx, logger.Object, authService.Object);
        var charges = await visitService.GetVisitChargesAsync(visit.VisitId);

        Assert.Equal(2, charges.Count);
        Assert.Equal("First", charges[0].ChargeDescription);
        Assert.Equal("Second", charges[1].ChargeDescription);
    }
}
