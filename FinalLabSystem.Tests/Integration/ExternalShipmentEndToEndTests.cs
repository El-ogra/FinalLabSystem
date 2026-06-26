using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Integration;

public class ExternalShipmentEndToEndTests
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

    private static (VisitTest vt, ExternalLab lab) SetupData(FinalLabDbContext ctx)
    {
        var lab = new ExternalLab { LabName = "Ext Lab", IsActive = true };
        ctx.ExternalLabs.Add(lab);

        var patient = new Patient { PatientCode = "P001", FullNameAr = "م", FullNameEn = "P", Sex = "M" };
        ctx.Patients.Add(patient);
        ctx.SaveChanges();

        var visit = new Visit { VisitCode = "V001", PatientId = patient.PatientId, VisitDate = DateTime.UtcNow, VisitStatus = VisitStatus.Open };
        ctx.Visits.Add(visit);
        ctx.SaveChanges();

        var tt = new TestType { TypeCode = "T01", TypeNameEn = "CBC", GroupId = 1, DefaultPrice = 50, IsActive = true };
        ctx.TestTypes.Add(tt);
        ctx.SaveChanges();

        var comp = new TestComponent { TesttypeId = tt.TesttypeId, ComponentCode = "WBC", ComponentNameEn = "WBC", ResultType = "Numeric", SortOrder = 1, IsActive = true };
        ctx.TestComponents.Add(comp);
        ctx.SaveChanges();

        var vt = new VisitTest { VisitId = visit.VisitId, TesttypeId = tt.TesttypeId, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow };
        ctx.VisitTests.Add(vt);
        ctx.SaveChanges();

        return (vt, lab);
    }

    [Fact]
    public async Task EndToEnd_ManifestSendReceive_TestResult()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_ManifestSendReceive_TestResult)));
        var (vt, lab) = SetupData(ctx);
        var service = CreateService(ctx);

        var shipment = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { vt.VisitTestId }, 0);
        Assert.Equal("PENDING", shipment.Status);
        Assert.Single(shipment.ExternalShipmentItems.ToList());

        var vtAfterSend = await ctx.VisitTests.FindAsync(vt.VisitTestId);
        Assert.Equal(TestStage.SentOut, vtAfterSend!.CurrentStage);

        var itemId = shipment.ExternalShipmentItems.First().ShipmentItemId;
        await service.ReceiveResultsAsync(itemId, "12.5", 0);

        var vtAfterReceive = await ctx.VisitTests.FindAsync(vt.VisitTestId);
        Assert.Equal(TestStage.ResultEntered, vtAfterReceive!.CurrentStage);

        var itemAfter = await ctx.ExternalShipmentItems.FindAsync(itemId);
        Assert.Equal("Received", itemAfter!.Status);

        var results = await ctx.TestResults.Where(r => r.VisitTestId == vt.VisitTestId).ToListAsync();
        Assert.Single(results);
        Assert.Equal("12.5", results[0].ResultValue);
    }

    [Fact]
    public async Task MultipleShipments_SameLab_ShouldBeIndependent()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(MultipleShipments_SameLab_ShouldBeIndependent)));
        var (vt1, lab) = SetupData(ctx);

        var patient2 = new Patient { PatientCode = "P002", FullNameAr = "م2", FullNameEn = "P2", Sex = "F" };
        ctx.Patients.Add(patient2);
        ctx.SaveChanges();
        var visit2 = new Visit { VisitCode = "V002", PatientId = patient2.PatientId, VisitDate = DateTime.UtcNow, VisitStatus = VisitStatus.Open };
        ctx.Visits.Add(visit2);
        ctx.SaveChanges();
        var vt2 = new VisitTest { VisitId = visit2.VisitId, TesttypeId = vt1.TesttypeId, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow };
        ctx.VisitTests.Add(vt2);
        ctx.SaveChanges();

        var service = CreateService(ctx);
        var s1 = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { vt1.VisitTestId }, 0);
        var s2 = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { vt2.VisitTestId }, 0);

        Assert.NotEqual(s1.ShipmentId, s2.ShipmentId);

        var shipments = await service.GetShipmentsAsync(lab.ExternalLabId);
        Assert.Equal(2, shipments.Count);
    }

    [Fact]
    public async Task UpdateStatus_ShouldPersist()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(UpdateStatus_ShouldPersist)));
        var (vt, lab) = SetupData(ctx);
        var service = CreateService(ctx);
        var shipment = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { vt.VisitTestId }, 0);

        await service.UpdateStatusAsync(shipment.ShipmentId, "SENT");

        var updated = await ctx.ExternalShipments.FindAsync(shipment.ShipmentId);
        Assert.Equal("SENT", updated!.Status);
    }

    [Fact]
    public async Task ReceiveResults_ShouldCreateMultipleComponents()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ReceiveResults_ShouldCreateMultipleComponents)));
        var lab = new ExternalLab { LabName = "L", IsActive = true };
        ctx.ExternalLabs.Add(lab);

        var patient = new Patient { PatientCode = "P001", FullNameAr = "م", FullNameEn = "P", Sex = "M" };
        ctx.Patients.Add(patient);
        ctx.SaveChanges();
        var visit = new Visit { VisitCode = "V001", PatientId = patient.PatientId, VisitDate = DateTime.UtcNow, VisitStatus = VisitStatus.Open };
        ctx.Visits.Add(visit);
        ctx.SaveChanges();

        var tt = new TestType { TypeCode = "T01", TypeNameEn = "CBC", GroupId = 1, DefaultPrice = 50, IsActive = true };
        ctx.TestTypes.Add(tt);
        ctx.SaveChanges();

        ctx.TestComponents.AddRange(
            new TestComponent { TesttypeId = tt.TesttypeId, ComponentCode = "WBC", ComponentNameEn = "WBC", ResultType = "Numeric", SortOrder = 1, IsActive = true },
            new TestComponent { TesttypeId = tt.TesttypeId, ComponentCode = "RBC", ComponentNameEn = "RBC", ResultType = "Numeric", SortOrder = 2, IsActive = true }
        );
        ctx.SaveChanges();

        var vt = new VisitTest { VisitId = visit.VisitId, TesttypeId = tt.TesttypeId, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow };
        ctx.VisitTests.Add(vt);
        ctx.SaveChanges();

        var service = CreateService(ctx);
        var shipment = await service.CreateManifestAsync(lab.ExternalLabId, new List<int> { vt.VisitTestId }, 0);
        var itemId = shipment.ExternalShipmentItems.First().ShipmentItemId;

        await service.ReceiveResultsAsync(itemId, "10", 0);

        var results = await ctx.TestResults.Where(r => r.VisitTestId == vt.VisitTestId).ToListAsync();
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task FullWorkflow_LabRegistryAndShipment()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(FullWorkflow_LabRegistryAndShipment)));
        var registryService = new ExternalLabRegistryService(ctx);
        var shipmentService = CreateService(ctx);

        var lab = await registryService.CreateAsync(new ExternalLab { LabName = "New Lab", IsActive = true });
        Assert.True(lab.ExternalLabId > 0);

        var allLabs = await registryService.GetAllAsync();
        Assert.Single(allLabs);

        var patient = new Patient { PatientCode = "P001", FullNameAr = "م", FullNameEn = "P", Sex = "M" };
        ctx.Patients.Add(patient);
        ctx.SaveChanges();
        var visit = new Visit { VisitCode = "V001", PatientId = patient.PatientId, VisitDate = DateTime.UtcNow, VisitStatus = VisitStatus.Open };
        ctx.Visits.Add(visit);
        ctx.SaveChanges();
        var tt = new TestType { TypeCode = "T01", TypeNameEn = "T", GroupId = 1, DefaultPrice = 50, IsActive = true };
        ctx.TestTypes.Add(tt);
        ctx.SaveChanges();
        var comp = new TestComponent { TesttypeId = tt.TesttypeId, ComponentCode = "C", ComponentNameEn = "C", ResultType = "Numeric", SortOrder = 1, IsActive = true };
        ctx.TestComponents.Add(comp);
        ctx.SaveChanges();
        var vt = new VisitTest { VisitId = visit.VisitId, TesttypeId = tt.TesttypeId, CurrentStage = TestStage.Pending, AddedAt = DateTime.UtcNow };
        ctx.VisitTests.Add(vt);
        ctx.SaveChanges();

        var shipment = await shipmentService.CreateManifestAsync(lab.ExternalLabId, new List<int> { vt.VisitTestId }, 0);
        var itemId = shipment.ExternalShipmentItems.First().ShipmentItemId;
        await shipmentService.ReceiveResultsAsync(itemId, "Result1", 0);

        var result = await ctx.TestResults.FirstAsync(r => r.VisitTestId == vt.VisitTestId);
        Assert.Equal("Result1", result.ResultValue);
    }
}
