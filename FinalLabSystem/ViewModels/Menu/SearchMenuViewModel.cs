using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Patients.Search;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class SearchMenuViewModel : ViewModelBase
{
    public SearchMenuViewModel(INavigationService navigationService)
    {
        NavigateToSearchCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<PatientSearchViewModel>());
    }

    public ICommand NavigateToSearchCommand { get; }
}
