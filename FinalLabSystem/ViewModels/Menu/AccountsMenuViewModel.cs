using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class AccountsMenuViewModel : ViewModelBase
{
    public AccountsMenuViewModel(INavigationService navigationService)
    {
        NavigateToCompaniesCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<CompaniesWindowViewModel>());
        NavigateToPricingCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<PriceSchemeWindowViewModel>());
        NavigateToInvoicesCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<ContractInvoiceWindowViewModel>());
        NavigateToCashDrawerCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<CashDrawerWindowViewModel>());
    }

    public ICommand NavigateToCompaniesCommand { get; }
    public ICommand NavigateToPricingCommand { get; }
    public ICommand NavigateToInvoicesCommand { get; }
    public ICommand NavigateToCashDrawerCommand { get; }
}
