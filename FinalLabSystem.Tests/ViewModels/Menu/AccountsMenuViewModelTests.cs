using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Menu;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Menu;

public class AccountsMenuViewModelTests
{
    [Fact]
    public void NavigateToCompaniesCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToCompaniesCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Settings.CompaniesWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToPricingCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToPricingCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Settings.PriceSchemeWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void NavigateToInvoicesCommand_ShouldCallOpenTaskWindow()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.NavigateToInvoicesCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<FinalLabSystem.ViewModels.Settings.ContractInvoiceWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void PlaceholderCommand_ShouldNotCallNavigation()
    {
        var navMock = new Mock<INavigationService>();
        var vm = new AccountsMenuViewModel(navMock.Object);

        vm.PlaceholderCommand.Execute(null);

        navMock.Verify(n => n.OpenTaskWindow<It.IsAnyType>(), Times.Never);
    }
}
