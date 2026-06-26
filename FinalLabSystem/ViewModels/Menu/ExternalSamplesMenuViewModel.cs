using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class ExternalSamplesMenuViewModel : ViewModelBase
{
    public ExternalSamplesMenuViewModel(INavigationService navigationService)
    {
        NavigateToExternalLabsCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<ExternalLabsWindowViewModel>());
    }

    public ICommand NavigateToExternalLabsCommand { get; }
}
