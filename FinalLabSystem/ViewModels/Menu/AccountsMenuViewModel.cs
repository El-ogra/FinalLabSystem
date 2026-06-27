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
        NavigateToCommissionReportCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<CommissionReportWindowViewModel>());
        NavigateToOutstandingBalanceCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<OutstandingBalanceWindowViewModel>());
    }

    public ICommand NavigateToCompaniesCommand { get; }
    public ICommand NavigateToPricingCommand { get; }
    public ICommand NavigateToInvoicesCommand { get; }
    public ICommand NavigateToCashDrawerCommand { get; }
    public ICommand NavigateToCommissionReportCommand { get; }
    public ICommand NavigateToOutstandingBalanceCommand { get; }
}
