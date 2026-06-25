using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class BackupMenuViewModel : ViewModelBase
{
    public BackupMenuViewModel(IDialogService dialogService)
    {
        PlaceholderCommand = new RelayCommand(_ => dialogService.ShowMessage("سيتم تفعيل هذه الميزة في المرحلة 6", "قريباً"));
    }

    public ICommand PlaceholderCommand { get; }
}
