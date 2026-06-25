using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;

namespace FinalLabSystem.ViewModels.Patients.Delivery;

public sealed class DeliveryViewModel : ViewModelBase
{
    public DeliveryViewModel(INavigationService navigationService)
    {
        ReturnToMainCommand = new RelayCommand(_ => navigationService.ReturnToMain());
    }

    public ICommand ReturnToMainCommand { get; }
}
