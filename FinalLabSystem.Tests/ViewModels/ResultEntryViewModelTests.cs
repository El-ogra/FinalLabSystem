using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class ResultEntryViewModelTests
{
    private static (ResultEntryViewModel vm, Mock<IRoutineResultService> mockRoutine,
        Mock<ICurrentUserSession> mockSession, Mock<IDialogService> mockDialog)
        CreateViewModel(int visitTestId = 1, int patientId = 1, string testTypeName = "CBC")
    {
        var mockRoutine = new Mock<IRoutineResultService>();
        var mockVisit = new Mock<IVisitService>();
        var mockAudit = new Mock<IAuditService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();

        var staff = new FinalLabSystem.Models.Staff
        {
            StaffId = 1,
            Username = "testuser",
            DisplayName = "Test User"
        };
        mockSession.Setup(s => s.CurrentUser).Returns(staff);

        var components = new ObservableCollection<TestComponentResultDto>
        {
            new() { ComponentId = 1, ComponentCode = "WBC", ComponentName = "White Blood Cells", ResultValue = "7.5" },
            new() { ComponentId = 2, ComponentCode = "RBC", ComponentName = "Red Blood Cells", ResultValue = "" }
        };

        var vm = new ResultEntryViewModel(
            mockRoutine.Object,
            mockVisit.Object,
            mockAudit.Object,
            mockSession.Object,
            visitTestId,
            patientId,
            testTypeName,
            components);

        return (vm, mockRoutine, mockSession, mockDialog);
    }

    // ========== Initialization Tests ==========

    [Fact]
    public void Constructor_SetsVisitTestId()
    {
        var (vm, _, _, _) = CreateViewModel(visitTestId: 42);

        Assert.Equal(42, vm.VisitTestId);
    }

    [Fact]
    public void Constructor_SetsTestTypeName()
    {
        var (vm, _, _, _) = CreateViewModel(testTypeName: "Glucose");

        Assert.Equal("Glucose", vm.TestTypeName);
    }

    [Fact]
    public void Constructor_SetsComponents()
    {
        var (vm, _, _, _) = CreateViewModel();

        Assert.NotNull(vm.Components);
        Assert.Equal(2, vm.Components.Count);
    }

    [Fact]
    public void Constructor_IsSavingIsFalse()
    {
        var (vm, _, _, _) = CreateViewModel();

        Assert.False(vm.IsSaving);
    }

    [Fact]
    public void Constructor_SelectedComponentIsNull()
    {
        var (vm, _, _, _) = CreateViewModel();

        Assert.Null(vm.SelectedComponent);
    }

    // ========== SaveCommand Tests ==========

    [Fact]
    public void SaveCommand_IsNotNull()
    {
        var (vm, _, _, _) = CreateViewModel();

        Assert.NotNull(vm.SaveCommand);
    }

    [Fact]
    public async Task SaveAsync_WithNoStaffId_DoesNotCallService()
    {
        var mockRoutine = new Mock<IRoutineResultService>();
        var mockVisit = new Mock<IVisitService>();
        var mockAudit = new Mock<IAuditService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();

        mockSession.Setup(s => s.CurrentUser).Returns((FinalLabSystem.Models.Staff?)null);

        var components = new ObservableCollection<TestComponentResultDto>
        {
            new() { ComponentId = 1, ResultValue = "7.5" }
        };

        var vm = new ResultEntryViewModel(
            mockRoutine.Object, mockVisit.Object, mockAudit.Object, mockSession.Object,
            1, 1, "CBC", components);

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        mockRoutine.Verify(s => s.SaveNumericOrTextResultsAsync(
            It.IsAny<List<FinalLabSystem.Models.TestResult>>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsync_WithResults_CallsSaveService()
    {
        var (vm, mockRoutine, _, _) = CreateViewModel();

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        mockRoutine.Verify(s => s.SaveNumericOrTextResultsAsync(
            It.IsAny<List<FinalLabSystem.Models.TestResult>>(),
            It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_SavesOnlyNonEmptyComponents()
    {
        var (vm, mockRoutine, _, _) = CreateViewModel();

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        mockRoutine.Verify(s => s.SaveNumericOrTextResultsAsync(
            It.Is<List<FinalLabSystem.Models.TestResult>>(r => r.Count == 1),
            It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_FiresSaveCompleted()
    {
        var (vm, _, _, _) = CreateViewModel();
        var saveCompletedFired = false;
        vm.SaveCompleted += (_, _) => saveCompletedFired = true;

        vm.SaveCommand.Execute(null);
        await Task.Delay(100);

        Assert.True(saveCompletedFired);
    }

    // ========== CancelCommand Tests ==========

    [Fact]
    public void CancelCommand_IsNotNull()
    {
        var (vm, _, _, _) = CreateViewModel();

        Assert.NotNull(vm.CancelCommand);
    }

    [Fact]
    public void CancelCommand_WhenNotSaving_InvokesRequestClose()
    {
        var (vm, _, _, _) = CreateViewModel();
        var closeRequested = false;
        vm.RequestClose = () => closeRequested = true;

        vm.CancelCommand.Execute(null);

        Assert.True(closeRequested);
    }

    [Fact]
    public void RequestClose_DefaultIsNull()
    {
        var (vm, _, _, _) = CreateViewModel();

        Assert.Null(vm.RequestClose);
    }

    [Fact]
    public void Components_CanBeReplaced()
    {
        var (vm, _, _, _) = CreateViewModel();
        var newComponents = new ObservableCollection<TestComponentResultDto>
        {
            new() { ComponentId = 99, ComponentCode = "NEW", ResultValue = "5" }
        };

        vm.Components = newComponents;

        Assert.Single(vm.Components);
        Assert.Equal(99, vm.Components[0].ComponentId);
    }

    [Fact]
    public void SelectedComponent_CanBeSetAndGets()
    {
        var (vm, _, _, _) = CreateViewModel();
        var component = vm.Components[0];

        vm.SelectedComponent = component;

        Assert.Same(component, vm.SelectedComponent);
    }
}
