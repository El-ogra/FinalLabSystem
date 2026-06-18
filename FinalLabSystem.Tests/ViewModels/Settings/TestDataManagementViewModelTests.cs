using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class TestDataManagementViewModelTests
{
    private readonly Mock<ITestCatalogService> _catalogServiceMock = new();
    private readonly Mock<IDialogService> _dialogServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<ILogger<TestDataManagementViewModel>> _loggerMock = new();

    private TestDataManagementViewModel CreateVm()
    {
        var testList = new TestListViewModel(_catalogServiceMock.Object);
        var testDetail = new TestDetailViewModel(_catalogServiceMock.Object, _dialogServiceMock.Object);
        return new TestDataManagementViewModel(
            testList,
            testDetail,
            _catalogServiceMock.Object,
            _navigationServiceMock.Object,
            _loggerMock.Object,
            _dialogServiceMock.Object);
    }

    [Fact]
    public void Constructor_InitializesChildViewModels()
    {
        var testList = new TestListViewModel(_catalogServiceMock.Object);
        var testDetail = new TestDetailViewModel(_catalogServiceMock.Object, _dialogServiceMock.Object);
        var vm = new TestDataManagementViewModel(
            testList,
            testDetail,
            _catalogServiceMock.Object,
            _navigationServiceMock.Object,
            _loggerMock.Object,
            _dialogServiceMock.Object);

        Assert.NotNull(vm.TestList);
        Assert.NotNull(vm.TestDetail);
        Assert.Same(testList, vm.TestList);
        Assert.Same(testDetail, vm.TestDetail);
    }

    [Fact]
    public async Task LoadAsync_PopulatesAllLists()
    {
        var tests = new List<TestType>
        {
            new() { TesttypeId = 1, TypeCode = "T001", TypeNameEn = "Test One", GroupId = 1, SortOrder = 1, TurnaroundHours = 24, IsActive = true },
            new() { TesttypeId = 2, TypeCode = "T002", TypeNameEn = "Test Two", GroupId = 1, SortOrder = 2, TurnaroundHours = 24, IsActive = true }
        };
        _catalogServiceMock.Setup(s => s.GetAllTestTypesAsync()).ReturnsAsync(tests);
        _catalogServiceMock.Setup(s => s.GetActiveGroupsAsync()).ReturnsAsync(new List<TestGroup>());
        _catalogServiceMock.Setup(s => s.GetAllCollectionTypesAsync()).ReturnsAsync(new List<CollectionType>());
        _catalogServiceMock.Setup(s => s.GetAllTubeMaterialsAsync()).ReturnsAsync(new List<TubeMaterial>());

        var vm = CreateVm();

        await vm.InitializeAsync();

        Assert.Equal(2, vm.TestList.AllTests.Count);
        Assert.Equal("T001", vm.TestList.AllTests[0].TypeCode);
        Assert.Equal("T002", vm.TestList.AllTests[1].TypeCode);
    }

    [Fact]
    public void IsBrowsing_TogglesCorrectly()
    {
        var vm = CreateVm();

        Assert.True(vm.IsBrowsing);

        vm.AddCommand.Execute(null);

        Assert.False(vm.IsBrowsing);
        Assert.True(vm.IsAdding);
        Assert.False(vm.IsEditing);

        vm.CancelCommand.Execute(null);

        Assert.True(vm.IsBrowsing);
        Assert.False(vm.IsAdding);
        Assert.False(vm.IsEditing);
    }

    [Fact]
    public void SaveCommand_WhenNotDirty_DoesNothing()
    {
        var vm = CreateVm();

        vm.SaveCommand.Execute(null);

        _catalogServiceMock.Verify(s => s.CreateTestTypeAsync(
            It.IsAny<TestType>(),
            It.IsAny<decimal>(),
            It.IsAny<decimal>(),
            It.IsAny<IReadOnlyList<TestTypeSampleTube>>()), Times.Never);
    }

    [Fact]
    public void AddCommand_CreatesNewTest()
    {
        var vm = CreateVm();

        vm.AddCommand.Execute(null);

        Assert.True(vm.TestDetail.IsDirty);
        Assert.Equal(string.Empty, vm.TestDetail.TypeCode);
        Assert.Equal((short)24, vm.TestDetail.EditableTest.TurnaroundHours);
        Assert.True(vm.IsAdding);
        Assert.False(vm.IsBrowsing);
        Assert.False(vm.IsEditing);
        Assert.Null(vm.TestList.SelectedTest);
    }

    [Fact]
    public void CancelCommand_ResetsToBrowsing()
    {
        var vm = CreateVm();
        vm.AddCommand.Execute(null);

        Assert.False(vm.IsBrowsing);

        vm.CancelCommand.Execute(null);

        Assert.True(vm.IsBrowsing);
        Assert.False(vm.IsAdding);
        Assert.False(vm.IsEditing);
    }
}
