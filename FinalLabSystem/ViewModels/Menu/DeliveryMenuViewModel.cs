using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Patients.Delivery;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class DeliveryMenuViewModel : ViewModelBase
{
    public DeliveryMenuViewModel(INavigationService navigationService)
    {
        NavigateToDeliveryCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<DeliveryViewModel>());
    }

    public ICommand NavigateToDeliveryCommand { get; }
}
