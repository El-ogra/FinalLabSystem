using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class TestListViewModelTests
{
    private readonly Mock<ITestCatalogService> _catalogServiceMock = new();

    private TestListViewModel CreateVm()
        => new(_catalogServiceMock.Object);

    private static List<TestType> CreateSampleTests()
    {
        var group = new TestGroup
        {
            GroupId = 1,
            GroupCode = "CHEM",
            GroupNameEn = "Chemistry",
            GroupNameAr = "كيمياء",
            SortOrder = 1,
            IsActive = true
        };
        var group2 = new TestGroup
        {
            GroupId = 2,
            GroupCode = "HEMA",
            GroupNameEn = "Hematology",
            GroupNameAr = "دم",
            SortOrder = 2,
            IsActive = true
        };

        return new List<TestType>
        {
            new()
            {
                TesttypeId = 1, TypeCode = "GLU", TypeNameEn = "Glucose", TypeNameAr = "جلوكوز",
                GroupId = 1, Group = group, SortOrder = 1, TurnaroundHours = 24, IsActive = true
            },
            new()
            {
                TesttypeId = 2, TypeCode = "CBC", TypeNameEn = "CBC", TypeNameAr = "صورة دم",
                GroupId = 2, Group = group2, SortOrder = 2, TurnaroundHours = 24, IsActive = true
            },
            new()
            {
                TesttypeId = 3, TypeCode = "CREA", TypeNameEn = "Creatinine", TypeNameAr = "كرياتينين",
                GroupId = 1, Group = group, SortOrder = 3, TurnaroundHours = 24, IsActive = true
            }
        };
    }

    [Fact]
    public async Task ApplyFilter_WithNoFilters_ShowsAllTests()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());
        await vm.RefreshAsync();

        Assert.Equal(3, vm.FilteredTests.Count);
    }

    [Fact]
    public async Task ApplyFilter_ByTestName_FiltersCorrectly()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());
        await vm.RefreshAsync();

        vm.SearchTestName = "glu";

        Assert.Single(vm.FilteredTests);
        Assert.Equal("GLU", vm.FilteredTests[0].TypeCode);
    }

    [Fact]
    public async Task ApplyFilter_ByTestNameArabic_FiltersCorrectly()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());
        await vm.RefreshAsync();

        vm.SearchTestName = "جلوكوز";

        Assert.Single(vm.FilteredTests);
        Assert.Equal("GLU", vm.FilteredTests[0].TypeCode);
    }

    [Fact]
    public async Task ApplyFilter_ByGroupName_FiltersCorrectly()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());
        await vm.RefreshAsync();

        vm.SearchGroupName = "Chemistry";

        Assert.Equal(2, vm.FilteredTests.Count);
    }

    [Fact]
    public async Task ApplyFilter_ByGroupNameArabic_FiltersCorrectly()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());
        await vm.RefreshAsync();

        vm.SearchGroupName = "كيمياء";

        Assert.Equal(2, vm.FilteredTests.Count);
    }

    [Fact]
    public async Task ApplyFilter_ByTestId_FiltersByTypeCode()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());
        await vm.RefreshAsync();

        vm.SearchTestId = "GLU";

        Assert.Single(vm.FilteredTests);
        Assert.Equal("GLU", vm.FilteredTests[0].TypeCode);
    }

    [Fact]
    public async Task ApplyFilter_WithAllFilters_CombinesCorrectly()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());
        await vm.RefreshAsync();

        vm.SearchTestName = "glu";
        vm.SearchGroupName = "Chemistry";

        Assert.Single(vm.FilteredTests);
        Assert.Equal("GLU", vm.FilteredTests[0].TypeCode);
    }

    [Fact]
    public async Task RefreshAsync_LoadsAndFiltersTests()
    {
        var vm = CreateVm();
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(CreateSampleTests());

        await vm.RefreshAsync();

        Assert.Equal(3, vm.AllTests.Count);
        Assert.Equal(3, vm.FilteredTests.Count);
    }

    [Fact]
    public void SelectedTestChanged_FiresOnSelection()
    {
        var vm = CreateVm();
        var testType = CreateSampleTests()[0];
        var row = new TestRowViewModel(testType);
        TestRowViewModel? firedEvent = null;
        vm.SelectedTestChanged += (_, row) => firedEvent = row;

        vm.SelectedTest = row;

        Assert.Same(row, firedEvent);
    }

    [Fact]
    public void SelectedTestChanged_FiresNullOnDeselection()
    {
        var vm = CreateVm();
        TestRowViewModel? firedEvent = null;
        vm.SelectedTestChanged += (_, row) => firedEvent = row;

        vm.SelectedTest = null;

        Assert.Null(firedEvent);
    }
}
