using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Menu;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Menu;

public class AccountsMenuViewModelTests
{
    [Fact]
    public void NavigateToCompaniesCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToCompaniesCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<CompaniesWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToPricingCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToPricingCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<PriceSchemeWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToInvoicesCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToInvoicesCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<ContractInvoiceWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToCashDrawerCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToCashDrawerCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<CashDrawerWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToCommissionReportCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToCommissionReportCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<CommissionReportWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToOutstandingBalanceCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToOutstandingBalanceCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<OutstandingBalanceWindowViewModel>(), Times.Once);
    }
}
