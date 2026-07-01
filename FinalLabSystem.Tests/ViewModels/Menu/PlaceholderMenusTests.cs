using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Menu;
using FinalLabSystem.Views.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels;

public class PlaceholderMenusTests
{
    private static (ExternalSamplesMenuViewModel vm, Mock<INavigationService> mockNav)
        CreateExternalSamplesVM()
    {
        var mockNav = new Mock<INavigationService>();
        var vm = new ExternalSamplesMenuViewModel(mockNav.Object);
        return (vm, mockNav);
    }

    private static (AccountsMenuViewModel vm, Mock<INavigationService> mockNav)
        CreateAccountsVM()
    {
        var mockNav = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(mockNav.Object);
        return (vm, mockNav);
    }

    private static (BackupMenuViewModel vm, Mock<IDialogService> mockDialog)
        CreateBackupVM()
    {
        var mockDialog = new Mock<IDialogService>();
        var vm = new BackupMenuViewModel(mockDialog.Object);
        return (vm, mockDialog);
    }

    private static (ReportSettingsMenuViewModel vm, Mock<INavigationService> mockNavigation)
        CreateReportSettingsVM()
    {
        var mockNavigation = new Mock<INavigationService>();
        var vm = new ReportSettingsMenuViewModel(mockNavigation.Object);
        return (vm, mockNavigation);
    }

    [Fact]
    public void ExternalSamples_NavigateToExternalLabsCommand_OpensNavigation()
    {
        var (vm, mockNav) = CreateExternalSamplesVM();

        vm.NavigateToExternalLabsCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Settings.ExternalLabsWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void Accounts_NavigateToCashDrawerCommand_OpensNavigation()
    {
        var (vm, mockNav) = CreateAccountsVM();

        vm.NavigateToCashDrawerCommand.Execute(null);

        mockNav.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Settings.CashDrawerWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void Backup_OpenBackupCommand_ShowsCustomDialog()
    {
        var (vm, mockDialog) = CreateBackupVM();

        vm.OpenBackupCommand.Execute(null);

        mockDialog.Verify(d => d.ShowCustomDialog<BackupRestoreWindow>(), Times.Once);
    }

    [Fact]
    public void ReportSettings_ManageTemplatesCommand_OpensNavigation()
    {
        var (vm, mockNavigation) = CreateReportSettingsVM();

        vm.ManageTemplatesCommand.Execute(null);

        mockNavigation.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Settings.ReportCommentTemplateViewModel>(), Times.Once);
    }
}
