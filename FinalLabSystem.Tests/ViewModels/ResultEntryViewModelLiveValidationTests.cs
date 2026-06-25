using System.Collections.ObjectModel;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.ViewModels.Patients;
using Moq;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Tests.ViewModels;

public class ResultEntryViewModelLiveValidationTests
{
    private static (ResultEntryViewModel vm, Mock<IDialogService> mockDialog)
        CreateViewModel(
            ObservableCollection<TestComponentResultDto>? components = null,
            int patientAgeDays = 0,
            string patientGender = "U",
            bool isPregnant = false)
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

        components ??= new ObservableCollection<TestComponentResultDto>();

        var vm = new ResultEntryViewModel(
            mockRoutine.Object,
            mockVisit.Object,
            mockAudit.Object,
            mockSession.Object,
            mockDialog.Object,
            1, 1, "CBC",
            components,
            patientAgeDays,
            patientGender,
            isPregnant);

        return (vm, mockDialog);
    }

    [Fact]
    public void Component_WithNormalValue_ClinicalStatusIsNormal()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "1.0",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5
            }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.Equal(ResultClinicalStatus.Normal, vm.Components[0].ClinicalStatus);
    }

    [Fact]
    public void Component_WithHighValue_ClinicalStatusIsHigh()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "2.0",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5
            }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.Equal(ResultClinicalStatus.High, vm.Components[0].ClinicalStatus);
    }

    [Fact]
    public void Component_WithLowValue_ClinicalStatusIsLow()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "0.1",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5
            }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.Equal(ResultClinicalStatus.Low, vm.Components[0].ClinicalStatus);
    }

    [Fact]
    public void Component_WithHighCriticalValue_ClinicalStatusIsCritical()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "3.0",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5,
                SnapHighCritical = 2.5
            }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.Equal(ResultClinicalStatus.Critical, vm.Components[0].ClinicalStatus);
    }

    [Fact]
    public void Component_WithLowCriticalValue_ClinicalStatusIsCritical()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "0.05",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5,
                SnapLowCritical = 0.2
            }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.Equal(ResultClinicalStatus.Critical, vm.Components[0].ClinicalStatus);
    }

    [Fact]
    public void Component_WithNoResult_ClinicalStatusIsNormal()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5
            }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.Equal(ResultClinicalStatus.Normal, vm.Components[0].ClinicalStatus);
    }

    [Fact]
    public void Component_ResultValueChange_UpdatesClinicalStatus()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "1.0",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5
            }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.Equal(ResultClinicalStatus.Normal, vm.Components[0].ClinicalStatus);

        vm.Components[0].ResultValue = "2.0";

        Assert.Equal(ResultClinicalStatus.High, vm.Components[0].ClinicalStatus);
    }

    [Fact]
    public void IsSelectedForSave_DefaultsToTrue()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new() { ComponentId = 1, ResultValue = "1.0" }
        };

        var (vm, _) = CreateViewModel(components);

        Assert.True(vm.Components[0].IsSelectedForSave);
    }

    [Fact]
    public void SaveAndReviewCommand_IsNotNull()
    {
        var (vm, _) = CreateViewModel();

        Assert.NotNull(vm.SaveAndReviewCommand);
    }

    [Fact]
    public void CriticalSave_PromptsConfirmation()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "3.0",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5,
                SnapHighCritical = 2.5,
                IsSelectedForSave = true
            }
        };

        var (vm, mockDialog) = CreateViewModel(components);

        mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        vm.SaveCommand.Execute(null);

        mockDialog.Verify(d => d.ShowConfirmation(
            It.Is<string>(s => s.Contains("حرجة")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void CancelledCriticalConfirmation_PreventsSave()
    {
        var components = new ObservableCollection<TestComponentResultDto>
        {
            new()
            {
                ComponentId = 1,
                ResultValue = "3.0",
                SnapLowNormal = 0.5,
                SnapHighNormal = 1.5,
                SnapHighCritical = 2.5,
                IsSelectedForSave = true
            }
        };

        var (vm, mockDialog) = CreateViewModel(components);

        mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var saveCompleted = false;
        vm.SaveCompleted += (_, _) => saveCompleted = true;

        vm.SaveCommand.Execute(null);

        Assert.False(saveCompleted);
    }

    [Fact]
    public void CustomEditorContent_IsNull()
    {
        var (vm, _) = CreateViewModel();

        Assert.Null(vm.CustomEditorContent);
    }
}
