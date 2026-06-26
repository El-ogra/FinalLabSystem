using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class ExternalShipmentServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static ExternalShipmentService CreateService(FinalLabDbContext ctx)
    {
        var logger = new Mock<ILogger<ExternalShipmentService>>();
        return new ExternalShipmentService(ctx, logger.Object);
    }

    private static ExternalLab CreateLab(FinalLabDbContext ctx, string name = "Test Lab")
    {
        var lab = new ExternalLab { LabName = name, IsActive = true };
        ctx.ExternalLabs.Add(lab);
        ctx.SaveChanges();
        return lab;
    }

    private static (Visit visit, VisitTest visitTest) CreateVisitTest(FinalLabDbContext ctx)
    {
        var patient = new Patient
        {
            PatientCode = "P001",
            FullNameAr = "جون",
            FullNameEn = "John Doe",
            Sex = "M",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };
        ctx.Patients.Add(patient);
        ctx.SaveChanges();

        var visit = new Visit
        {
            VisitCode = "V001",
            PatientId = patient.PatientId,
            VisitDate = DateTime.UtcNow,
            VisitStatus = VisitStatus.Open
        };
        ctx.Visits.Add(visit);
        ctx.SaveChanges();

        var testType = new TestType
        {
            TypeCode = "T001",
            TypeNameEn = "CBC",
            TypeNameAr = "تعداد دموي",
            GroupId = 1,
            DefaultPrice = 50,
            IsActive = true
        };
        ctx.TestTypes.Add(testType);
        ctx.SaveChanges();

        var component = new TestComponent
        {
            TesttypeId = testType.TesttypeId,
            ComponentCode = "WBC",
            ComponentNameEn = "WBC",
            ResultType = "Numeric",
            SortOrder = 1,
            IsActive = true
        };
        ctx.TestComponents.Add(component);
        ctx.SaveChanges();

        var visitTest = new VisitTest
        {
            VisitId = visit.VisitId,
            TesttypeId = testType.TesttypeId,
            CurrentStage = TestStage.Pending,
            AddedAt = DateTime.UtcNow
        };
        ctx.VisitTests.Add(visitTest);
        ctx.SaveChanges();

        return (visit, visitTest);
    }

    [Fact]
    public async Task CreateManifestAsync_ShouldCreateShipmentWithItems()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateManifestAsync_ShouldCreateShipmentWithItems)));
        var lab = CreateLab(ctx);
        var (visit, visitTest) = CreateVisitTest(ctx);

        var service = CreateService(ctx);
        var shipment = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { visitTest.VisitTestId }, 0);

        Assert.True(shipment.ShipmentId > 0);
        Assert.Equal("PENDING", shipment.Status);
        Assert.Single(shipment.ExternalShipmentItems.ToList());
        Assert.Equal(visitTest.VisitTestId, shipment.ExternalShipmentItems.First().VisitTestId);

        var updatedVisitTest = await ctx.VisitTests.FindAsync(visitTest.VisitTestId);
        Assert.Equal(TestStage.SentOut, updatedVisitTest!.CurrentStage);
        Assert.True(updatedVisitTest.IsOutsourced);
    }

    [Fact]
    public async Task CreateManifestAsync_ShouldThrow_WhenLabNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateManifestAsync_ShouldThrow_WhenLabNotFound)));
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateManifestAsync(999, new List<int> { 1 }, 0));
    }

    [Fact]
    public async Task CreateManifestAsync_ShouldThrow_WhenVisitTestNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(CreateManifestAsync_ShouldThrow_WhenVisitTestNotFound)));
        var lab = CreateLab(ctx);
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateManifestAsync(lab.ExternalLabId, new List<int> { 999 }, 0));
    }

    [Fact]
    public async Task GetShipmentsAsync_ShouldReturnShipmentsForLab()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetShipmentsAsync_ShouldReturnShipmentsForLab)));
        var lab1 = CreateLab(ctx, "Lab 1");
        var lab2 = CreateLab(ctx, "Lab 2");
        var (v1, vt1) = CreateVisitTest(ctx);
        var (v2, vt2) = CreateVisitTest(ctx);

        var service = CreateService(ctx);
        await service.CreateManifestAsync(lab1.ExternalLabId, new List<int> { vt1.VisitTestId }, 0);
        await service.CreateManifestAsync(lab2.ExternalLabId, new List<int> { vt2.VisitTestId }, 0);

        var shipments = await service.GetShipmentsAsync(lab1.ExternalLabId);

        Assert.Single(shipments);
        Assert.Equal(lab1.ExternalLabId, shipments[0].ExternalLabId);
    }

    [Fact]
    public async Task ReceiveResultsAsync_ShouldCreateTestResultsAndMarkReceived()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ReceiveResultsAsync_ShouldCreateTestResultsAndMarkReceived)));
        var lab = CreateLab(ctx);
        var (visit, visitTest) = CreateVisitTest(ctx);

        var service = CreateService(ctx);
        var shipment = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { visitTest.VisitTestId }, 0);
        var itemId = shipment.ExternalShipmentItems.First().ShipmentItemId;

        await service.ReceiveResultsAsync(itemId, "12.5", 0);

        var item = await ctx.ExternalShipmentItems.FindAsync(itemId);
        Assert.Equal("Received", item!.Status);

        var visitTestAfter = await ctx.VisitTests.FindAsync(visitTest.VisitTestId);
        Assert.Equal(TestStage.ResultEntered, visitTestAfter!.CurrentStage);
        Assert.NotNull(visitTestAfter.OutsourceResultReceivedAt);

        var results = await ctx.TestResults.Where(r => r.VisitTestId == visitTest.VisitTestId).ToListAsync();
        Assert.Single(results);
        Assert.Equal("12.5", results[0].ResultValue);
    }

    [Fact]
    public async Task ReceiveResultsAsync_ShouldThrow_WhenItemNotFound()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ReceiveResultsAsync_ShouldThrow_WhenItemNotFound)));
        var service = CreateService(ctx);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ReceiveResultsAsync(999, "10", 0));
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateShipmentStatus()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateStatusAsync_ShouldUpdateShipmentStatus)));
        var lab = CreateLab(ctx);
        var (visit, visitTest) = CreateVisitTest(ctx);

        var service = CreateService(ctx);
        var shipment = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { visitTest.VisitTestId }, 0);

        await service.UpdateStatusAsync(shipment.ShipmentId, "SENT");

        var updated = await ctx.ExternalShipments.FindAsync(shipment.ShipmentId);
        Assert.Equal("SENT", updated!.Status);
    }
}
