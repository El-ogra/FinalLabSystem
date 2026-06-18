using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class NormalRangeListViewModelTests
{
    private readonly Mock<ITestCatalogService> _catalogServiceMock = new();
    private readonly Mock<IDialogService> _dialogServiceMock = new();
    private readonly NormalRangeDetailViewModel _detail;

    public NormalRangeListViewModelTests()
    {
        _detail = new NormalRangeDetailViewModel(
            _catalogServiceMock.Object, _dialogServiceMock.Object);
    }

    private NormalRangeListViewModel CreateVm()
        => new(_catalogServiceMock.Object, _detail, _dialogServiceMock.Object);

    private static List<TestComponent> CreateSampleComponents()
    {
        return new List<TestComponent>
        {
            new()
            {
                ComponentId = 1,
                TesttypeId = 1,
                ComponentCode = "GLU",
                ComponentNameEn = "Glucose",
                ResultType = "NUMERIC",
                Unit = "mg/dL",
                IsActive = true,
                SortOrder = 1
            },
            new()
            {
                ComponentId = 2,
                TesttypeId = 1,
                ComponentCode = "HGB",
                ComponentNameEn = "Hemoglobin",
                ResultType = "NUMERIC",
                Unit = "g/dL",
                IsActive = true,
                SortOrder = 2
            }
        };
    }

    [Fact]
    public void LoadComponents_PopulatesListSorted()
    {
        var vm = CreateVm();
        var components = CreateSampleComponents();

        vm.LoadComponents(components);

        Assert.Equal(2, vm.Components.Count);
        Assert.Equal("GLU", vm.Components[0].ComponentCode);
        Assert.Equal("HGB", vm.Components[1].ComponentCode);
    }

    [Fact]
    public void LoadComponents_SelectsFirstComponent()
    {
        var vm = CreateVm();

        vm.LoadComponents(CreateSampleComponents());

        Assert.NotNull(vm.SelectedComponent);
        Assert.Equal("GLU", vm.SelectedComponent.ComponentCode);
    }

    [Fact]
    public void AddRange_WithSelectedComponent_AddsToCollection()
    {
        var vm = CreateVm();
        vm.LoadComponents(CreateSampleComponents());

        vm.AddRangeCommand.Execute(null);

        Assert.Single(vm.RangesForSelectedComponent);
        var range = vm.RangesForSelectedComponent[0];
        Assert.Equal(vm.SelectedComponent!.ComponentId, range.ComponentId);
        Assert.Equal("Both", range.Sex);
        Assert.Equal("A", range.FastingState);
    }

    [Fact]
    public void AddRange_WithoutSelectedComponent_DoesNothing()
    {
        var vm = CreateVm();

        vm.AddRangeCommand.Execute(null);

        Assert.Empty(vm.RangesForSelectedComponent);
    }

    [Fact]
    public void DeleteRange_WithConfirmation_RemovesRange()
    {
        var vm = CreateVm();
        vm.LoadComponents(CreateSampleComponents());
        vm.AddRangeCommand.Execute(null);
        Assert.Single(vm.RangesForSelectedComponent);

        _dialogServiceMock.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        vm.DeleteRangeCommand.Execute(null);

        Assert.Empty(vm.RangesForSelectedComponent);
    }

    [Fact]
    public void DeleteRange_WithoutConfirmation_DoesNotRemove()
    {
        var vm = CreateVm();
        vm.LoadComponents(CreateSampleComponents());
        vm.AddRangeCommand.Execute(null);
        Assert.Single(vm.RangesForSelectedComponent);

        _dialogServiceMock.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        vm.DeleteRangeCommand.Execute(null);

        Assert.Single(vm.RangesForSelectedComponent);
    }

    [Fact]
    public void DeleteRange_WithoutSelection_DoesNothing()
    {
        var vm = CreateVm();

        vm.DeleteRangeCommand.Execute(null);

        Assert.Empty(vm.RangesForSelectedComponent);
    }

    [Fact]
    public void AddComponent_AddsNewComponent()
    {
        var vm = CreateVm();
        vm.LoadComponents(CreateSampleComponents());

        vm.AddComponentCommand.Execute(null);

        Assert.Equal(3, vm.Components.Count);
        Assert.Equal("COMP3", vm.Components[2].ComponentCode);
        Assert.Equal("New Component", vm.Components[2].ComponentNameEn);
        Assert.Equal("NUMERIC", vm.Components[2].ResultType);
        Assert.True(vm.Components[2].IsActive);
        Assert.Equal(3, vm.Components[2].SortOrder);
    }

    [Fact]
    public void AddComponent_WhenEmpty_SetsSortOrderToOne()
    {
        var vm = CreateVm();

        vm.AddComponentCommand.Execute(null);

        Assert.Single(vm.Components);
        Assert.Equal(1, vm.Components[0].SortOrder);
    }

    [Fact]
    public void SelectedRange_LoadsIntoDetail()
    {
        var vm = CreateVm();
        vm.LoadComponents(CreateSampleComponents());

        var range = new NormalRange
        {
            RangeId = 1, ComponentId = 1, Sex = "B",
            AgeFromDays = 0, AgeToDays = 100, FastingState = "A"
        };
        vm.RangesForSelectedComponent.Add(range);

        vm.SelectedRange = range;

        Assert.Equal(range.RangeId, _detail.EditableRange.RangeId);
    }
}
