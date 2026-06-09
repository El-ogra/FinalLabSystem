using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private object? _currentView;

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        ShowPatientsMenuCommand = new RelayCommand(_ => ShowPatientsMenu());
        ShowSystemSettingsMenuCommand = new RelayCommand(_ => ShowSystemSettingsMenu());
        NavigateToAddEditPatientCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<PatientRegistrationViewModel>());
        NavigateToTestResultsCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<TestResultsViewModel>());
        NavigateToDeliveryCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<DeliveryViewModel>());
        NavigateToSearchCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<PatientSearchViewModel>());
        NavigateToTestDataCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<TestDataManagementViewModel>());
        NavigateToCategoriesGroupsCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<CategoriesGroupsViewModel>());
    }

    public object? CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public ICommand ShowPatientsMenuCommand { get; }

    public ICommand ShowSystemSettingsMenuCommand { get; }

    public ICommand NavigateToAddEditPatientCommand { get; }

    public ICommand NavigateToTestResultsCommand { get; }

    public ICommand NavigateToDeliveryCommand { get; }

    public ICommand NavigateToSearchCommand { get; }

    public ICommand NavigateToTestDataCommand { get; }

    public ICommand NavigateToCategoriesGroupsCommand { get; }

    private void ShowPatientsMenu()
    {
        CurrentView = new PatientsMenuViewModel(
            NavigateToAddEditPatientCommand,
            NavigateToTestResultsCommand,
            NavigateToDeliveryCommand,
            NavigateToSearchCommand);
    }

    private void ShowSystemSettingsMenu()
    {
        CurrentView = new SystemSettingsMenuViewModel(NavigateToTestDataCommand, NavigateToCategoriesGroupsCommand);
    }
}

public sealed class PatientsMenuViewModel
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
