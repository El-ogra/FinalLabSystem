using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class NormalRangeWindowViewModelTests
{
    private readonly Mock<ITestCatalogService> _catalogServiceMock = new();
    private readonly Mock<IDialogService> _dialogServiceMock = new();
    private NormalRangeDetailViewModel _detail = null!;
    private NormalRangeListViewModel _list = null!;

    private NormalRangeWindowViewModel CreateVm()
    {
        _detail = new NormalRangeDetailViewModel(_catalogServiceMock.Object, _dialogServiceMock.Object);
        _list = new NormalRangeListViewModel(_catalogServiceMock.Object, _detail, _dialogServiceMock.Object);
        return new NormalRangeWindowViewModel(_list, _detail, _catalogServiceMock.Object);
    }

    private static TestType CreateTestTypeWithNoComponents()
    {
        return new TestType
        {
            TesttypeId = 1,
            TypeCode = "T001",
            TypeNameEn = "Test One",
            GroupId = 1,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            TestComponents = new List<TestComponent>()
        };
    }

    private static TestType CreateTestTypeWithComponents()
    {
        return new TestType
        {
            TesttypeId = 2,
            TypeCode = "T002",
            TypeNameEn = "Test Two",
            GroupId = 1,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true,
            TestComponents = new List<TestComponent>
            {
                new()
                {
                    ComponentId = 1,
                    TesttypeId = 2,
                    ComponentCode = "GLU",
                    ComponentNameEn = "Glucose",
                    ResultType = "NUMERIC",
                    Unit = "mg/dL",
                    IsActive = true,
                    SortOrder = 1,
                    NormalRanges = new List<NormalRange>
                    {
                        new()
                        {
                            RangeId = 1,
                            ComponentId = 1,
                            Sex = "B",
                            AgeFromDays = 0,
                            AgeToDays = 36500,
                            LowNormal = 0.5,
                            HighNormal = 1.5,
                            FastingState = "A",
                            Unit = "mg/dL"
                        }
                    }
                }
            }
        };
    }

    [Fact]
    public async Task InitializeAsync_WithNoComponents_LoadsEmptyList()
    {
        var vm = CreateVm();
        var parentTest = CreateTestTypeWithNoComponents();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(1)).ReturnsAsync(parentTest);

        await vm.InitializeAsync(parentTest);

        Assert.Empty(vm.List.Components);
    }

    [Fact]
    public async Task InitializeAsync_WithExistingComponents_LoadsThem()
    {
        var vm = CreateVm();
        var parentTest = CreateTestTypeWithComponents();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(2)).ReturnsAsync(parentTest);

        await vm.InitializeAsync(parentTest);

        Assert.Single(vm.List.Components);
        Assert.Equal(1, vm.List.Components[0].ComponentId);
        Assert.Equal("GLU", vm.List.Components[0].ComponentCode);
    }

    [Fact]
    public async Task InitializeAsync_SetsParentTestAndTitle()
    {
        var vm = CreateVm();
        var parentTest = CreateTestTypeWithComponents();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(parentTest.TesttypeId)).ReturnsAsync(parentTest);

        await vm.InitializeAsync(parentTest);

        Assert.NotNull(vm.ParentTest);
        Assert.Equal("Test Two", vm.ParentTest.TypeNameEn);
        Assert.Equal("القيم الطبيعية - Test Two", vm.Title);
    }

    [Fact]
    public async Task InitializeAsync_SetsReferenceCount()
    {
        var vm = CreateVm();
        var parentTest = CreateTestTypeWithComponents();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(parentTest.TesttypeId)).ReturnsAsync(parentTest);
        _catalogServiceMock.Setup(s => s.GetRangesForComponentAsync(1))
            .ReturnsAsync(parentTest.TestComponents.First().NormalRanges.ToList());

        await vm.InitializeAsync(parentTest);

        Assert.Equal(1, vm.ReferenceCount);
    }

    [Fact]
    public void SaveCommand_DelegatesToDetail()
    {
        var vm = CreateVm();
        Assert.Same(vm.Detail.SaveCommand, vm.SaveCommand);
    }

    [Fact]
    public void CancelCommand_DelegatesToDetail()
    {
        var vm = CreateVm();
        Assert.Same(vm.Detail.CancelCommand, vm.CancelCommand);
    }

    [Fact]
    public void AddRangeCommand_DelegatesToList()
    {
        var vm = CreateVm();
        Assert.Same(vm.List.AddRangeCommand, vm.AddRangeCommand);
    }

    [Fact]
    public async Task ReferenceCount_UpdatesWhenRangesChanged()
    {
        var vm = CreateVm();
        var parentTest = CreateTestTypeWithComponents();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(parentTest.TesttypeId)).ReturnsAsync(parentTest);
        _catalogServiceMock.Setup(s => s.GetRangesForComponentAsync(1))
            .ReturnsAsync(parentTest.TestComponents.First().NormalRanges.ToList());

        await vm.InitializeAsync(parentTest);

        Assert.Equal(1, vm.ReferenceCount);

        vm.List.AddRangeCommand.Execute(null);

        Assert.Equal(2, vm.ReferenceCount);
    }

    [Fact]
    public async Task InitializeAsync_SetsComponentsFromTest()
    {
        var vm = CreateVm();
        var parentTest = CreateTestTypeWithComponents();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(2)).ReturnsAsync(parentTest);

        await vm.InitializeAsync(parentTest);

        var component = Assert.Single(vm.List.Components);
        Assert.Equal("GLU", component.ComponentCode);
        Assert.Equal("Glucose", component.ComponentNameEn);
        Assert.Equal("NUMERIC", component.ResultType);
        Assert.Equal("mg/dL", component.Unit);
    }

    [Fact]
    public async Task InitializeAsync_SetsReferenceCountToZeroWhenEmpty()
    {
        var vm = CreateVm();
        var parentTest = CreateTestTypeWithNoComponents();
        _catalogServiceMock.Setup(s => s.GetTestTypeDetailsAsync(1)).ReturnsAsync(parentTest);

        await vm.InitializeAsync(parentTest);

        Assert.Equal(0, vm.ReferenceCount);
    }
}
