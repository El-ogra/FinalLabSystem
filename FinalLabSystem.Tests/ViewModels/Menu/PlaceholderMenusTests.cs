using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Menu;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class PlaceholderMenusTests
{
    private static (ExternalSamplesMenuViewModel vm, Mock<IDialogService> mockDialog)
        CreateExternalSamplesVM()
    {
        var mockDialog = new Mock<IDialogService>();
        var vm = new ExternalSamplesMenuViewModel(mockDialog.Object);
        return (vm, mockDialog);
    }

    private static (AccountsMenuViewModel vm, Mock<IDialogService> mockDialog)
        CreateAccountsVM()
    {
        var mockDialog = new Mock<IDialogService>();
        var vm = new AccountsMenuViewModel(mockDialog.Object);
        return (vm, mockDialog);
    }

    private static (BackupMenuViewModel vm, Mock<IDialogService> mockDialog)
        CreateBackupVM()
    {
        var mockDialog = new Mock<IDialogService>();
        var vm = new BackupMenuViewModel(mockDialog.Object);
        return (vm, mockDialog);
    }

    private static (ReportSettingsMenuViewModel vm, Mock<IDialogService> mockDialog)
        CreateReportSettingsVM()
    {
        var mockDialog = new Mock<IDialogService>();
        var vm = new ReportSettingsMenuViewModel(mockDialog.Object);
        return (vm, mockDialog);
    }

    [Fact]
    public void ExternalSamples_PlaceholderCommand_ShowsPhase4Dialog()
    {
        var (vm, mockDialog) = CreateExternalSamplesVM();

        vm.PlaceholderCommand.Execute(null);

        mockDialog.Verify(d => d.ShowMessage(
            It.Is<string>(s => s.Contains("المرحلة 4")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Accounts_PlaceholderCommand_ShowsPhase5Dialog()
    {
        var (vm, mockDialog) = CreateAccountsVM();

        vm.PlaceholderCommand.Execute(null);

        mockDialog.Verify(d => d.ShowMessage(
            It.Is<string>(s => s.Contains("المرحلة 5")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Backup_PlaceholderCommand_ShowsPhase6Dialog()
    {
        var (vm, mockDialog) = CreateBackupVM();

        vm.PlaceholderCommand.Execute(null);

        mockDialog.Verify(d => d.ShowMessage(
            It.Is<string>(s => s.Contains("المرحلة 6")),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ReportSettings_PlaceholderCommand_ShowsPhase6Dialog()
    {
        var (vm, mockDialog) = CreateReportSettingsVM();

        vm.PlaceholderCommand.Execute(null);

        mockDialog.Verify(d => d.ShowMessage(
            It.Is<string>(s => s.Contains("المرحلة 6")),
            It.IsAny<string>()), Times.Once);
    }
}
