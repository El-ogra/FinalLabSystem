using System.Windows.Input;
using FinalLabSystem.ViewModels.Menu;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class PatientsMenuViewModelTests
{
    private static PatientsMenuViewModel CreateViewModel()
    {
        var mockAddEdit = new Mock<ICommand>();
        var mockTestResults = new Mock<ICommand>();
        var mockDelivery = new Mock<ICommand>();
        var mockSearch = new Mock<ICommand>();

        return new PatientsMenuViewModel(
            mockAddEdit.Object,
            mockTestResults.Object,
            mockDelivery.Object,
            mockSearch.Object);
    }

    [Fact]
    public void NavigateToAddEditPatientCommand_IsNotNull()
    {
        var vm = CreateViewModel();

        Assert.NotNull(vm.NavigateToAddEditPatientCommand);
    }

    [Fact]
    public void NavigateToTestResultsCommand_IsNotNull()
    {
        var vm = CreateViewModel();

        Assert.NotNull(vm.NavigateToTestResultsCommand);
    }

    [Fact]
    public void NavigateToDeliveryCommand_IsNotNull()
    {
        var vm = CreateViewModel();

        Assert.NotNull(vm.NavigateToDeliveryCommand);
    }

    [Fact]
    public void NavigateToSearchCommand_IsNotNull()
    {
        var vm = CreateViewModel();

        Assert.NotNull(vm.NavigateToSearchCommand);
    }
}
