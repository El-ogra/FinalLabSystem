using FinalLabSystem.Models;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class TestRowViewModelTests
{
    private static TestGroup CreateGroup(string en = "Group", string? ar = null)
        => new()
        {
            GroupId = 1,
            GroupCode = "GRP",
            GroupNameEn = en,
            GroupNameAr = ar ?? en,
            SortOrder = 1,
            IsActive = true
        };

    [Fact]
    public void DisplayName_ReturnsArabicWhenAvailable()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Glucose",
            TypeNameAr = "جلوكوز",
            GroupId = 1,
            Group = CreateGroup(),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal("جلوكوز", vm.DisplayName);
    }

    [Fact]
    public void DisplayName_FallsBackToEnglish()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Glucose",
            TypeNameAr = null,
            GroupId = 1,
            Group = CreateGroup(),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal("Glucose", vm.DisplayName);
    }

    [Fact]
    public void GroupNameEn_ReturnsFromGroup()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup("Chemistry", "كيمياء"),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal("Chemistry", vm.GroupNameEn);
    }

    [Fact]
    public void GroupNameAr_ReturnsFromGroup()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup("Chemistry", "كيمياء"),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal("كيمياء", vm.GroupNameAr);
    }

    [Fact]
    public void GroupNameAr_FallsBackToEnglish()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup("Chemistry", null),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal("Chemistry", vm.GroupNameAr);
    }

    [Fact]
    public void PatientPrice_ReturnsFromPrices()
    {
        var scheme = new PriceScheme { SchemeId = 1, SchemeName = "Patient Price", IsActive = true, CreatedAt = DateTime.UtcNow };
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup(),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            DefaultPrice = 100m,
            TestTypePrices = new List<TestTypePrice>
            {
                new() { PriceId = 1, TesttypeId = 1, SchemeId = 1, Scheme = scheme, Price = 80m }
            }
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(80m, vm.PatientPrice);
    }

    [Fact]
    public void PatientPrice_FallsBackToDefaultPrice()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup(),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            DefaultPrice = 100m,
            TestTypePrices = new List<TestTypePrice>()
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(100m, vm.PatientPrice);
    }

    [Fact]
    public void LabToLabPrice_ReturnsFromPrices()
    {
        var scheme = new PriceScheme { SchemeId = 2, SchemeName = "Lab-to-Lab Price", IsActive = true, CreatedAt = DateTime.UtcNow };
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup(),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            TestTypePrices = new List<TestTypePrice>
            {
                new() { PriceId = 1, TesttypeId = 1, SchemeId = 2, Scheme = scheme, Price = 40m }
            }
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(40m, vm.LabToLabPrice);
    }

    [Fact]
    public void LabToLabPrice_DefaultsToZero()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup(),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            TestTypePrices = new List<TestTypePrice>()
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(0m, vm.LabToLabPrice);
    }

    [Fact]
    public void TubeCount_CountsActiveTubes()
    {
        var test = new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test",
            GroupId = 1,
            Group = CreateGroup(),
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            TestTypeSampleTubes = new List<TestTypeSampleTube>
            {
                new() { TestTypeTubeId = 1, TestTypeId = 1, SampleType = "Serum", Quantity = 1, SortOrder = 1, IsActive = true, TubeType = "Default" },
                new() { TestTypeTubeId = 2, TestTypeId = 1, SampleType = "Plasma", Quantity = 1, SortOrder = 2, IsActive = false, TubeType = "Default" },
                new() { TestTypeTubeId = 3, TestTypeId = 1, SampleType = "Citrate", Quantity = 1, SortOrder = 3, IsActive = true, TubeType = "Default" }
            }
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(2, vm.TubeCount);
    }

    [Fact]
    public void TypeCode_ReturnsCode()
    {
        var test = new TestType
        {
            TesttypeId = 1, TypeCode = "GLU", TypeNameEn = "Glucose",
            GroupId = 1, Group = CreateGroup(), SortOrder = 1, TurnaroundHours = 24, IsActive = true
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal("GLU", vm.TypeCode);
    }

    [Fact]
    public void SortOrder_ReturnsOrder()
    {
        var test = new TestType
        {
            TesttypeId = 1, TypeCode = "T001", TypeNameEn = "Test",
            GroupId = 1, Group = CreateGroup(), SortOrder = 5, TurnaroundHours = 24, IsActive = true
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(5, vm.SortOrder);
    }

    [Fact]
    public void Barcode_ReturnsBarcodeName()
    {
        var test = new TestType
        {
            TesttypeId = 1, TypeCode = "T001", TypeNameEn = "Test",
            GroupId = 1, Group = CreateGroup(), SortOrder = 1, TurnaroundHours = 24, IsActive = true,
            BarcodeName = "BAR001"
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal("BAR001", vm.Barcode);
    }

    [Fact]
    public void Barcode_WhenNull_ReturnsEmpty()
    {
        var test = new TestType
        {
            TesttypeId = 1, TypeCode = "T001", TypeNameEn = "Test",
            GroupId = 1, Group = CreateGroup(), SortOrder = 1, TurnaroundHours = 24, IsActive = true,
            BarcodeName = null
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(string.Empty, vm.Barcode);
    }

    [Fact]
    public void Constructor_MapsAllProperties()
    {
        var schemePatient = new PriceScheme { SchemeId = 1, SchemeName = "Patient Price", IsActive = true, CreatedAt = DateTime.UtcNow };
        var schemeLab = new PriceScheme { SchemeId = 2, SchemeName = "Lab-to-Lab Price", IsActive = true, CreatedAt = DateTime.UtcNow };
        var test = new TestType
        {
            TesttypeId = 42,
            TypeCode = "GLU",
            TypeNameEn = "Glucose",
            TypeNameAr = "جلوكوز",
            GroupId = 5,
            Group = new TestGroup
            {
                GroupId = 5,
                GroupCode = "CHM",
                GroupNameEn = "Chemistry",
                GroupNameAr = "كيمياء",
                SortOrder = 1,
                IsActive = true
            },
            SortOrder = 10,
            DefaultPrice = 200m,
            TestTypePrices = new List<TestTypePrice>
            {
                new() { PriceId = 1, TesttypeId = 42, SchemeId = 1, Scheme = schemePatient, Price = 150m },
                new() { PriceId = 2, TesttypeId = 42, SchemeId = 2, Scheme = schemeLab, Price = 75m }
            },
            IsActive = true,
            OutsideLabName = "External Lab",
            OutsideCostPrice = 60m,
            BarcodeName = "BAR042"
        };

        var vm = new TestRowViewModel(test);

        Assert.Equal(42, vm.TesttypeId);
        Assert.Equal("GLU", vm.TypeCode);
        Assert.Equal("Glucose", vm.TypeNameEn);
        Assert.Equal(5, vm.TestType.GroupId);
        Assert.Equal("Chemistry", vm.GroupNameEn);
        Assert.Equal("كيمياء", vm.GroupNameAr);
        Assert.Equal(10, vm.SortOrder);
        Assert.Equal(150m, vm.PatientPrice);
        Assert.Equal(75m, vm.LabToLabPrice);
        Assert.True(vm.TestType.IsActive);
        Assert.Equal("External Lab", vm.OutsideLabName);
        Assert.Equal(60m, vm.OutsideCostPrice);
        Assert.Equal("BAR042", vm.Barcode);
    }
}
