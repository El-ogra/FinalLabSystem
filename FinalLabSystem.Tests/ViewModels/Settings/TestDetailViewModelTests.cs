using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class TestDetailViewModelTests
{
    private readonly Mock<ITestCatalogService> _catalogServiceMock = new();
    private readonly Mock<IDialogService> _dialogServiceMock = new();

    private TestDetailViewModel CreateVm()
        => new(_catalogServiceMock.Object, _dialogServiceMock.Object);

    private static TestType CreateSampleTest(int id = 1)
    {
        var group = new TestGroup
        {
            GroupId = 1,
            GroupCode = "GRP",
            GroupNameEn = "Group",
            GroupNameAr = "مجموعة",
            SortOrder = 1,
            IsActive = true
        };

        var priceSchemePatient = new PriceScheme
        {
            SchemeId = 1,
            SchemeName = "Patient Price",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var priceSchemeLab = new PriceScheme
        {
            SchemeId = 2,
            SchemeName = "Lab-to-Lab Price",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return new TestType
        {
            TesttypeId = id,
            TypeCode = "T001",
            TypeNameEn = "Test One",
            TypeNameAr = "اختبار واحد",
            TypeAbbrev = "T1",
            GroupId = 1,
            Group = group,
            SortOrder = 5,
            TurnaroundHours = 24,
            IsActive = true,
            DefaultPrice = 100m,
            ReportNameLine1 = "Report 1",
            ReportNameLine2 = "Report 2",
            BillNameLine1 = "Bill 1",
            BillNameLine2 = "Bill 2",
            HistoryName = "History",
            CollectionNotes = "Collect notes",
            Notes = "Some notes",
            IsRoutineTest = true,
            SeeReport = true,
            PrintWithOther = false,
            AddWithGroup = true,
            IsMainTest = false,
            IsSendOutside = true,
            OutsideLabName = "Outside Lab",
            OutsideCostPrice = 50m,
            BarcodeName = "BAR001",
            PatientQuestion = "Any allergies?",
            ReferenceType = "Numeric Range",
            CollectionTypeId = 1,
            TestTypePrices = new List<TestTypePrice>
            {
                new() { PriceId = 1, TesttypeId = id, SchemeId = 1, Scheme = priceSchemePatient, Price = 80m },
                new() { PriceId = 2, TesttypeId = id, SchemeId = 2, Scheme = priceSchemeLab, Price = 40m }
            },
            TestTypeSampleTubes = new List<TestTypeSampleTube>
            {
                new() { TestTypeTubeId = 1, TestTypeId = id, SampleType = "Serum", Quantity = 1, SortOrder = 1, IsActive = true, TubeType = "Default" },
                new() { TestTypeTubeId = 2, TestTypeId = id, SampleType = "Plasma", Quantity = 1, SortOrder = 2, IsActive = true, TubeType = "Default" }
            },
            TestComponents = new List<TestComponent>
            {
                new() { ComponentId = 10, TesttypeId = id, ComponentCode = "T001", ComponentNameEn = "Hemoglobin", Unit = "g/dL", ResultType = "NUMERIC", DecimalPlaces = (byte)1, SortOrder = 1, IsActive = true },
                new() { ComponentId = 11, TesttypeId = id, ComponentCode = "T001", ComponentNameEn = "WBC", Unit = "x10^3/uL", ResultType = "NUMERIC", DecimalPlaces = (byte)2, SortOrder = 2, IsActive = true }
            }
        };
    }

    [Fact]
    public async Task LoadAsync_PopulatesFieldsFromLoadedTest()
    {
        var vm = CreateVm();
        var test = CreateSampleTest();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(1)).ReturnsAsync(test);

        await vm.LoadAsync(1, Array.Empty<TestRowViewModel>());

        Assert.Equal("T001", vm.TypeCode);
        Assert.Equal("Test One", vm.TypeNameEn);
        Assert.Equal("اختبار واحد", vm.TypeNameAr);
        Assert.Equal("History", vm.HistoryName);
        Assert.Equal("Report 1", vm.ReportNameLine1);
        Assert.Equal("Report 2", vm.ReportNameLine2);
        Assert.Equal("Bill 1", vm.BillNameLine1);
        Assert.Equal("Bill 2", vm.BillNameLine2);
        Assert.Equal(1, vm.GroupId);
        Assert.Equal(5, vm.SortOrder);
        Assert.Equal((short)24, vm.TurnaroundHours);
        Assert.Equal(80m, vm.PatientPrice);
        Assert.Equal(40m, vm.LabToLabPrice);
        Assert.True(vm.IsRoutineTest);
        Assert.True(vm.SeeReport);
        Assert.False(vm.PrintWithOther);
        Assert.True(vm.AddWithGroup);
        Assert.False(vm.IsMainTest);
        Assert.True(vm.IsSendOutside);
        Assert.True(vm.IsOutsideFieldsEnabled);
        Assert.Equal("Outside Lab", vm.OutsideLabName);
        Assert.Equal(50m, vm.OutsideCostPrice);
        Assert.Equal("Serum", vm.Tube1);
        Assert.Equal("Plasma", vm.Tube2);
        Assert.Equal("", vm.Tube3);
        Assert.Equal("Numeric Range", vm.ReferenceType);
        Assert.Equal("BAR001", vm.BarcodeName);
        Assert.Equal("Any allergies?", vm.PatientQuestion);
        Assert.Equal("Collect notes", vm.CollectionNotes);
        Assert.Equal("Some notes", vm.Notes);
        Assert.Equal(1, vm.SelectedCollectionTypeId);
    }

    [Fact]
    public async Task LoadAsync_SetsDirtyFalse()
    {
        var vm = CreateVm();
        var test = CreateSampleTest();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(1)).ReturnsAsync(test);

        await vm.LoadAsync(1, Array.Empty<TestRowViewModel>());

        Assert.False(vm.IsDirty);
    }

    [Fact]
    public void Validate_WithMissingTypeCode_ReturnsFalse()
    {
        var vm = CreateVm();

        var result = vm.Validate();

        Assert.False(result);
        Assert.NotNull(vm.ValidationMessage);
    }

    [Fact]
    public async Task Validate_WithDuplicateTypeCode_ReturnsFalse()
    {
        var vm = CreateVm();
        var existingTest = new TestType { TesttypeId = 2, TypeCode = "T001", TypeNameEn = "Other", GroupId = 1, SortOrder = 1, TurnaroundHours = 24, IsActive = true };
        var existingRows = new[] { new TestRowViewModel(existingTest) };

        var test = CreateSampleTest();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(1)).ReturnsAsync(test);
        await vm.LoadAsync(1, existingRows);

        vm.TypeCode = "t001";

        var result = vm.Validate();

        Assert.False(result);
    }

    [Fact]
    public void Validate_WithMissingName_ReturnsFalse()
    {
        var vm = CreateVm();
        vm.TypeCode = "T002";

        var result = vm.Validate();

        Assert.False(result);
    }

    [Fact]
    public void Validate_WithInvalidGroup_ReturnsFalse()
    {
        var vm = CreateVm();
        vm.TypeCode = "T002";
        vm.TypeNameEn = "Test";

        var result = vm.Validate();

        Assert.False(result);
    }

    [Fact]
    public void Validate_WithValidData_ReturnsTrue()
    {
        var vm = CreateVm();
        vm.TypeCode = "T002";
        vm.TypeNameEn = "Test";
        vm.GroupId = 1;

        var result = vm.Validate();

        Assert.True(result);
        Assert.Null(vm.ValidationMessage);
    }

    [Fact]
    public void BuildEntity_MapsAllFields()
    {
        var vm = CreateVm();
        vm.TypeCode = "T001";
        vm.TypeNameEn = "Test One";
        vm.TypeNameAr = "اختبار";
        vm.HistoryName = "History";
        vm.ReportNameLine1 = "R1";
        vm.ReportNameLine2 = "R2";
        vm.BillNameLine1 = "B1";
        vm.BillNameLine2 = "B2";
        vm.GroupId = 1;
        vm.SortOrder = 5;
        vm.TurnaroundHours = 48;
        vm.SelectedCollectionTypeId = 2;
        vm.CollectionNotes = "Notes";
        vm.Notes = "Some notes";
        vm.PatientPrice = 100m;
        vm.LabToLabPrice = 50m;
        vm.IsRoutineTest = true;
        vm.SeeReport = true;
        vm.PrintWithOther = false;
        vm.AddWithGroup = true;
        vm.IsMainTest = false;
        vm.IsSendOutside = true;
        vm.OutsideLabName = "Ext Lab";
        vm.OutsideCostPrice = 30m;
        vm.BarcodeName = "BAR001";
        vm.PatientQuestion = "Q?";
        vm.ReferenceType = "Numeric";

        var entity = vm.BuildEntity();

        Assert.Equal("T001", entity.TypeCode);
        Assert.Equal("Test One", entity.TypeNameEn);
        Assert.Equal("اختبار", entity.TypeNameAr);
        Assert.Equal("History", entity.HistoryName);
        Assert.Equal("R1", entity.ReportNameLine1);
        Assert.Equal("R2", entity.ReportNameLine2);
        Assert.Equal("B1", entity.BillNameLine1);
        Assert.Equal("B2", entity.BillNameLine2);
        Assert.Equal(2, entity.CollectionTypeId);
        Assert.Equal("Notes", entity.CollectionNotes);
        Assert.Equal("Some notes", entity.Notes);
        Assert.Equal("Ext Lab", entity.OutsideLabName);
        Assert.Equal(30m, entity.OutsideCostPrice);
        Assert.Equal("Q?", entity.PatientQuestion);
        Assert.Equal("Numeric", entity.ReferenceType);
        Assert.Equal("BAR001", entity.BarcodeName);
        Assert.Equal(100m, entity.DefaultPrice);
        Assert.True(entity.IsActive);
    }

    [Fact]
    public void BuildEntity_WithIsSendOutsideFalse_ClearsOutsideFields()
    {
        var vm = CreateVm();
        vm.TypeCode = "T001";
        vm.TypeNameEn = "Test";
        vm.GroupId = 1;
        vm.IsSendOutside = false;
        vm.OutsideLabName = "Should Be Null";
        vm.OutsideCostPrice = 100m;

        var entity = vm.BuildEntity();

        Assert.Null(entity.OutsideLabName);
        Assert.Null(entity.OutsideCostPrice);
    }

    [Fact]
    public void BuildTubes_CreatesTubeListFromFields()
    {
        var vm = CreateVm();
        vm.Tube1 = "Serum";
        vm.Tube2 = "Plasma";
        vm.Tube3 = "Citrate Blood";

        var tubes = vm.BuildTubes();

        Assert.Equal(3, tubes.Count);
        Assert.Equal("Serum", tubes[0].SampleType);
        Assert.Equal(1, tubes[0].SortOrder);
        Assert.Equal(1, tubes[0].Quantity);
        Assert.Equal("Serum", tubes[0].TubeType);
        Assert.Equal("Plasma", tubes[1].SampleType);
        Assert.Equal(2, tubes[1].SortOrder);
        Assert.Equal("Citrate Blood", tubes[2].SampleType);
        Assert.Equal(3, tubes[2].SortOrder);
    }

    [Fact]
    public void BuildTubes_WithEmptyTubes_ReturnsEmptyList()
    {
        var vm = CreateVm();
        vm.Tube1 = "";
        vm.Tube2 = "  ";
        vm.Tube3 = null!;

        var tubes = vm.BuildTubes();

        Assert.Empty(tubes);
    }

    [Fact]
    public void StartNew_ResetsToEmptyTest()
    {
        var vm = CreateVm();

        vm.StartNew(Array.Empty<TestRowViewModel>());

        Assert.Equal(string.Empty, vm.TypeCode);
        Assert.Equal(string.Empty, vm.TypeNameEn);
        Assert.Equal(24, vm.EditableTest.TurnaroundHours);
        Assert.True(vm.IsDirty);
    }

    [Fact]
    public async Task CancelChanges_RestoresAllFields()
    {
        var vm = CreateVm();
        var test = CreateSampleTest();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(1)).ReturnsAsync(test);

        await vm.LoadAsync(1, Array.Empty<TestRowViewModel>());

        vm.TypeCode = "CHANGED";
        vm.TypeNameEn = "Changed";
        vm.Tube1 = "ChangedTube";
        vm.ReferenceType = "Changed Ref";
        vm.BarcodeName = "Changed Barcode";

        vm.CancelChanges();

        Assert.Equal("T001", vm.TypeCode);
        Assert.Equal("Test One", vm.TypeNameEn);
        Assert.Equal("Serum", vm.Tube1);
        Assert.Equal("Numeric Range", vm.ReferenceType);
        Assert.Equal("BAR001", vm.BarcodeName);
        Assert.False(vm.IsDirty);
    }

    [Fact]
    public async Task LoadAsync_PopulatesComponents()
    {
        var vm = CreateVm();
        var test = CreateSampleTest();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(1)).ReturnsAsync(test);

        await vm.LoadAsync(1, Array.Empty<TestRowViewModel>());

        Assert.Equal(2, vm.Components.Count);
        Assert.Equal("Hemoglobin", vm.Components[0].ComponentNameEn);
        Assert.Equal("WBC", vm.Components[1].ComponentNameEn);
        Assert.Equal(2, vm.Components[1].SortOrder);
    }

    [Fact]
    public void AddComponent_AddsToCollection()
    {
        var vm = CreateVm();
        vm.StartNew(Array.Empty<TestRowViewModel>());

        vm.AddComponentCommand.Execute(null);

        Assert.Single(vm.Components);
        Assert.NotNull(vm.SelectedComponent);
        Assert.Equal("NUMERIC", vm.Components[0].ResultType);
        Assert.True(vm.IsDirty);
    }

    [Fact]
    public void DeleteComponent_RemovesFromCollection()
    {
        var vm = CreateVm();
        vm.StartNew(Array.Empty<TestRowViewModel>());
        vm.AddComponentCommand.Execute(null);
        Assert.Single(vm.Components);

        vm.DeleteComponentCommand.Execute(null);

        Assert.Empty(vm.Components);
        Assert.Null(vm.SelectedComponent);
    }

    [Fact]
    public void MoveComponentUp_Down_ReordersSortOrders()
    {
        var vm = CreateVm();
        vm.StartNew(Array.Empty<TestRowViewModel>());
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentNameEn = "First";
        vm.Components[1].ComponentNameEn = "Second";

        vm.SelectedComponent = vm.Components[1];
        vm.MoveComponentUpCommand.Execute(null);

        Assert.Equal("Second", vm.Components[0].ComponentNameEn);
        Assert.Equal("First", vm.Components[1].ComponentNameEn);
        Assert.Equal(1, vm.Components[0].SortOrder);
        Assert.Equal(2, vm.Components[1].SortOrder);

        vm.MoveComponentDownCommand.Execute(null);

        Assert.Equal("First", vm.Components[0].ComponentNameEn);
        Assert.Equal("Second", vm.Components[1].ComponentNameEn);
        Assert.Equal(1, vm.Components[0].SortOrder);
        Assert.Equal(2, vm.Components[1].SortOrder);
    }

    [Fact]
    public void BuildComponents_ReturnsValidList()
    {
        var vm = CreateVm();
        vm.StartNew(Array.Empty<TestRowViewModel>());
        vm.AddComponentCommand.Execute(null);
        vm.AddComponentCommand.Execute(null);

        var result = vm.BuildComponents();

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(vm.EditableTest.TypeCode, c.ComponentCode));
        Assert.All(result, c => Assert.Equal(vm.EditableTest.TypeNameEn, c.ComponentNameEn));
        Assert.All(result, c => Assert.Equal(vm.EditableTest.TesttypeId, c.TesttypeId));
    }
}
