using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class ResultsMenuViewModel : ViewModelBase
{
    public ResultsMenuViewModel(INavigationService navigationService)
    {
        NavigateToTestResultsCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<TestResultsViewModel>());
    }

    public ICommand NavigateToTestResultsCommand { get; }
}
