using System.Windows.Input;
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class PatientsMenuViewModel : ViewModelBase
{
    public PatientsMenuViewModel(
        ICommand navigateToAddEditPatientCommand,
        ICommand navigateToTestResultsCommand,
        ICommand navigateToDeliveryCommand,
        ICommand navigateToSearchCommand)
    {
        NavigateToAddEditPatientCommand = navigateToAddEditPatientCommand;
        NavigateToTestResultsCommand = navigateToTestResultsCommand;
        NavigateToDeliveryCommand = navigateToDeliveryCommand;
        NavigateToSearchCommand = navigateToSearchCommand;
    }

    public ICommand NavigateToAddEditPatientCommand { get; }

    public ICommand NavigateToTestResultsCommand { get; }

    public ICommand NavigateToDeliveryCommand { get; }

    public ICommand NavigateToSearchCommand { get; }
}
