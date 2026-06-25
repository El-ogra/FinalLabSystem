using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class ExternalSamplesMenuViewModel : ViewModelBase
{
    public ExternalSamplesMenuViewModel(IDialogService dialogService)
    {
        PlaceholderCommand = new RelayCommand(_ => dialogService.ShowMessage("سيتم تفعيل هذه الميزة في المرحلة 4", "قريباً"));
    }

    public ICommand PlaceholderCommand { get; }
}
