using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels;
using FinalLabSystem.ViewModels.Menu;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class MainViewModelTests
{
    private static (MainViewModel vm, Mock<INavigationService> mockNav, Mock<IDialogService> mockDialog)
        CreateViewModel()
    {
        var mockNav = new Mock<INavigationService>();
        var mockDialog = new Mock<IDialogService>();
        var mockInventory = new Mock<IInventoryService>();
        var vm = new MainViewModel(mockNav.Object, mockDialog.Object, mockInventory.Object);
        return (vm, mockNav, mockDialog);
    }

    // ========== Menu Commands — Show correct VM type ==========

    [Fact]
    public void ShowHomeMenuCommand_SetsHomeMenuViewModel()
    {
        var (vm, _, _) = CreateViewModel();

        vm.ShowHomeMenuCommand.Execute(null);

        Assert.IsType<HomeMenuViewModel>(vm.CurrentView);
    }

    [Fact]
    public void ShowPatientsMenuCommand_SetsPatientsMenuViewModel()
    {
        var (vm, _, _) = CreateViewModel();

        vm.ShowPatientsMenuCommand.Execute(null);

        Assert.IsType<PatientsMenuViewModel>(vm.CurrentView);
    }

    [Fact]
    public void ShowExternalSamplesMenuCommand_SetsExternalSamplesMenuViewModel()
    {
        var (vm, _, _) = CreateViewModel();

        vm.ShowExternalSamplesMenuCommand.Execute(null);

        Assert.IsType<ExternalSamplesMenuViewModel>(vm.CurrentView);
    }

    [Fact]
    public void ShowAccountsMenuCommand_SetsAccountsMenuViewModel()
    {
        var (vm, _, _) = CreateViewModel();

        vm.ShowAccountsMenuCommand.Execute(null);

        Assert.IsType<AccountsMenuViewModel>(vm.CurrentView);
    }

    [Fact]
    public void ShowBackupMenuCommand_SetsBackupMenuViewModel()
    {
        var (vm, _, _) = CreateViewModel();

        vm.ShowBackupMenuCommand.Execute(null);

        Assert.IsType<BackupMenuViewModel>(vm.CurrentView);
    }

    [Fact]
    public void ShowReportSettingsMenuCommand_SetsReportSettingsMenuViewModel()
    {
        var (vm, _, _) = CreateViewModel();

        vm.ShowReportSettingsMenuCommand.Execute(null);

        Assert.IsType<ReportSettingsMenuViewModel>(vm.CurrentView);
    }

    [Fact]
    public void ShowSystemSettingsMenuCommand_SetsSystemSettingsMenuViewModel()
    {
        var (vm, _, _) = CreateViewModel();

        vm.ShowSystemSettingsMenuCommand.Execute(null);

        Assert.IsType<SystemSettingsMenuViewModel>(vm.CurrentView);
    }

    // ========== Navigate Commands — Call navigation service ==========

    [Fact]
    public void NavigateToAddEditPatientCommand_CallsOpenTaskWindow()
    {
        var (vm, mockNav, _) = CreateViewModel();

        vm.NavigateToAddEditPatientCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Patients.PatientRegistrationViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToTestResultsCommand_CallsOpenTaskWindow()
    {
        var (vm, mockNav, _) = CreateViewModel();

        vm.NavigateToTestResultsCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Patients.TestResultsViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToDeliveryCommand_CallsOpenTaskWindow()
    {
        var (vm, mockNav, _) = CreateViewModel();

        vm.NavigateToDeliveryCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Patients.Delivery.DeliveryViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToSearchCommand_CallsOpenTaskWindow()
    {
        var (vm, mockNav, _) = CreateViewModel();

        vm.NavigateToSearchCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Patients.Search.PatientSearchViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToTestDataCommand_CallsOpenTaskWindow()
    {
        var (vm, mockNav, _) = CreateViewModel();

        vm.NavigateToTestDataCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<TestDataManagementViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToCategoriesGroupsCommand_CallsOpenTaskWindow()
    {
        var (vm, mockNav, _) = CreateViewModel();

        vm.NavigateToCategoriesGroupsCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<CategoriesGroupsViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToNormalRangesCommand_CallsOpenTaskWindow()
    {
        var (vm, mockNav, _) = CreateViewModel();

        vm.NavigateToNormalRangesCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<NormalRangeWindowViewModel>(), Times.Once);
    }

    // ========== Default State Tests ==========

    [Fact]
    public void CurrentView_DefaultIsNull()
    {
        var (vm, _, _) = CreateViewModel();

        Assert.Null(vm.CurrentView);
    }

    [Fact]
    public void AllCommands_AreNotNull()
    {
        var (vm, _, _) = CreateViewModel();

        Assert.NotNull(vm.ShowHomeMenuCommand);
        Assert.NotNull(vm.ShowPatientsMenuCommand);
        Assert.NotNull(vm.ShowSystemSettingsMenuCommand);
        Assert.NotNull(vm.ShowExternalSamplesMenuCommand);
        Assert.NotNull(vm.ShowAccountsMenuCommand);
        Assert.NotNull(vm.ShowBackupMenuCommand);
        Assert.NotNull(vm.ShowReportSettingsMenuCommand);
        Assert.NotNull(vm.NavigateToAddEditPatientCommand);
        Assert.NotNull(vm.NavigateToTestResultsCommand);
        Assert.NotNull(vm.NavigateToDeliveryCommand);
        Assert.NotNull(vm.NavigateToSearchCommand);
        Assert.NotNull(vm.NavigateToTestDataCommand);
        Assert.NotNull(vm.NavigateToCategoriesGroupsCommand);
        Assert.NotNull(vm.NavigateToNormalRangesCommand);
    }
}
