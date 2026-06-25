using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Menu;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.ViewModels.Patients.Delivery;
using FinalLabSystem.ViewModels.Patients.Search;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private object? _currentView;

    public MainViewModel(INavigationService navigationService, IDialogService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;

        ShowHomeMenuCommand = new RelayCommand(_ => CurrentView = new HomeMenuViewModel());
        ShowPatientsMenuCommand = new RelayCommand(_ => ShowPatientsMenu());
        ShowSystemSettingsMenuCommand = new RelayCommand(_ => ShowSystemSettingsMenu());
        ShowExternalSamplesMenuCommand = new RelayCommand(_ => CurrentView = new ExternalSamplesMenuViewModel(_dialogService));
        ShowAccountsMenuCommand = new RelayCommand(_ => CurrentView = new AccountsMenuViewModel(_dialogService));
        ShowBackupMenuCommand = new RelayCommand(_ => CurrentView = new BackupMenuViewModel(_dialogService));
        ShowReportSettingsMenuCommand = new RelayCommand(_ => CurrentView = new ReportSettingsMenuViewModel(_navigationService));

        NavigateToAddEditPatientCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<PatientRegistrationViewModel>());
        NavigateToTestResultsCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<TestResultsViewModel>());
        NavigateToDeliveryCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<DeliveryViewModel>());
        NavigateToSearchCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<PatientSearchViewModel>());
        NavigateToTestDataCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<TestDataManagementViewModel>());
        NavigateToCategoriesGroupsCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<CategoriesGroupsViewModel>());
        NavigateToNormalRangesCommand = new RelayCommand(_ => _navigationService.OpenTaskWindow<NormalRangeWindowViewModel>());
    }

    public object? CurrentView
    {
        get => _currentView;
        private set => SetProperty(ref _currentView, value);
    }

    public ICommand ShowHomeMenuCommand { get; }

    public ICommand ShowPatientsMenuCommand { get; }

    public ICommand ShowSystemSettingsMenuCommand { get; }

    public ICommand ShowExternalSamplesMenuCommand { get; }

    public ICommand ShowAccountsMenuCommand { get; }

    public ICommand ShowBackupMenuCommand { get; }

    public ICommand ShowReportSettingsMenuCommand { get; }

    public ICommand NavigateToAddEditPatientCommand { get; }

    public ICommand NavigateToTestResultsCommand { get; }

    public ICommand NavigateToDeliveryCommand { get; }

    public ICommand NavigateToSearchCommand { get; }

    public ICommand NavigateToTestDataCommand { get; }

    public ICommand NavigateToCategoriesGroupsCommand { get; }

    public ICommand NavigateToNormalRangesCommand { get; }

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
