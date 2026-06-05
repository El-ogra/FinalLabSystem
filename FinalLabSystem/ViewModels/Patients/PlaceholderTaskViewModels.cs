using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;

namespace FinalLabSystem.ViewModels.Patients;

public abstract class PlaceholderTaskViewModelBase : ViewModelBase
{
    protected PlaceholderTaskViewModelBase(INavigationService navigationService)
    {
        ReturnToMainCommand = new RelayCommand(_ => navigationService.ReturnToMain());
    }

    public ICommand ReturnToMainCommand { get; }
}

public sealed class TestResultsViewModel : PlaceholderTaskViewModelBase
{
    public TestResultsViewModel(INavigationService navigationService)
        : base(navigationService)
    {
    }
}

public sealed class DeliveryViewModel : PlaceholderTaskViewModelBase
{
    public DeliveryViewModel(INavigationService navigationService)
        : base(navigationService)
    {
    }
}

public sealed class PatientSearchViewModel : PlaceholderTaskViewModelBase
{
    public PatientSearchViewModel(INavigationService navigationService)
        : base(navigationService)
    {
    }
}
