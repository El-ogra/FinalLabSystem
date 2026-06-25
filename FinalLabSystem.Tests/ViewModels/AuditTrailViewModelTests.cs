using FinalLabSystem.Models;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Tests.ViewModels;

public class AuditTrailViewModelTests
{
    private static List<AuditLog> CreateAuditLogEntries(int count = 3)
    {
        return Enumerable.Range(1, count).Select(i => new AuditLog
        {
            AuditId = i,
            TableName = "Patients",
            RecordId = i,
            Action = "UPDATE",
            FieldName = "Name",
            OldValue = $"Old{i}",
            NewValue = $"New{i}",
            ChangedAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();
    }

    private static List<VResultAuditTrail> CreateResultAuditEntries(int count = 3)
    {
        return Enumerable.Range(1, count).Select(i => new VResultAuditTrail
        {
            AuditId = i,
            ResultId = i,
            Action = "UPDATE",
            FieldName = "ResultValue",
            OldValue = $"10{i}",
            NewValue = $"20{i}",
            ChangedAt = DateTime.UtcNow.AddDays(-i),
            ChangedByName = "TestUser",
            VisitId = 1,
            VisitCode = "V001",
            PatientName = "Test Patient",
            TestType = "CBC",
            ComponentName = "WBC"
        }).ToList();
    }

    // ========== AuditLog Overload Tests ==========

    [Fact]
    public void Constructor_AuditLogOverload_SetsTitleCorrectly()
    {
        var entries = CreateAuditLogEntries();

        var vm = new AuditTrailViewModel("Patient Audit Trail", entries);

        Assert.Equal("Patient Audit Trail", vm.Title);
    }

    [Fact]
    public void Constructor_AuditLogOverload_PopulatesEntries()
    {
        var entries = CreateAuditLogEntries(5);

        var vm = new AuditTrailViewModel("Audit", entries);

        Assert.NotNull(vm.Entries);
        Assert.Equal(5, vm.Entries.Count);
        Assert.Equal("UPDATE", vm.Entries[0].Action);
    }

    [Fact]
    public void Constructor_AuditLogOverload_EntriesViewIsNotNull()
    {
        var entries = CreateAuditLogEntries();

        var vm = new AuditTrailViewModel("Audit", entries);

        Assert.NotNull(vm.EntriesView);
    }

    [Fact]
    public void Constructor_AuditLogOverload_ResultEntriesIsNull()
    {
        var entries = CreateAuditLogEntries();

        var vm = new AuditTrailViewModel("Audit", entries);

        Assert.Null(vm.ResultEntries);
        Assert.Null(vm.ResultEntriesView);
    }

    // ========== VResultAuditTrail Overload Tests ==========

    [Fact]
    public void Constructor_ResultAuditOverload_SetsTitleCorrectly()
    {
        var entries = CreateResultAuditEntries();

        var vm = new AuditTrailViewModel("Result Audit Trail", entries);

        Assert.Equal("Result Audit Trail", vm.Title);
    }

    [Fact]
    public void Constructor_ResultAuditOverload_PopulatesResultEntries()
    {
        var entries = CreateResultAuditEntries(4);

        var vm = new AuditTrailViewModel("Result Audit", entries);

        Assert.NotNull(vm.ResultEntries);
        Assert.Equal(4, vm.ResultEntries.Count);
        Assert.Equal("TestUser", vm.ResultEntries[0].ChangedByName);
    }

    [Fact]
    public void Constructor_ResultAuditOverload_ResultEntriesViewIsNotNull()
    {
        var entries = CreateResultAuditEntries();

        var vm = new AuditTrailViewModel("Result Audit", entries);

        Assert.NotNull(vm.ResultEntriesView);
    }

    [Fact]
    public void Constructor_ResultAuditOverload_EntriesIsNull()
    {
        var entries = CreateResultAuditEntries();

        var vm = new AuditTrailViewModel("Result Audit", entries);

        Assert.Null(vm.Entries);
        Assert.Null(vm.EntriesView);
    }
}
