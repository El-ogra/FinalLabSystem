using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;

namespace FinalLabSystem.ViewModels.Patients.Search;

public sealed class PatientSearchViewModel : ViewModelBase
{
    public PatientSearchViewModel(INavigationService navigationService)
    {
        ReturnToMainCommand = new RelayCommand(_ => navigationService.ReturnToMain());
    }

    public ICommand ReturnToMainCommand { get; }
}
