using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class AccountsMenuViewModel : ViewModelBase
{
    public AccountsMenuViewModel(IDialogService dialogService)
    {
        PlaceholderCommand = new RelayCommand(_ => dialogService.ShowMessage("سيتم تفعيل هذه الميزة في المرحلة 5", "قريباً"));
    }

    public ICommand PlaceholderCommand { get; }
}
