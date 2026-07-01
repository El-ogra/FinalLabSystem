using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Menu;
using FinalLabSystem.Views.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Menu;

public class BackupMenuViewModelTests
{
    [Fact]
    public void OpenBackupCommand_CallsShowCustomDialog()
    {
        var mockDialog = new Mock<IDialogService>();
        var vm = new BackupMenuViewModel(mockDialog.Object);

        vm.OpenBackupCommand.Execute(null);

        mockDialog.Verify(d => d.ShowCustomDialog<BackupRestoreWindow>(), Times.Once);
    }
}
