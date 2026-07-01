using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class BackupMenuViewModel : ViewModelBase
{
    public BackupMenuViewModel(IDialogService dialogService)
    {
        OpenBackupCommand = new RelayCommand(_ =>
            dialogService.ShowCustomDialog<BackupRestoreWindow>());
    }

    public ICommand OpenBackupCommand { get; }
}
