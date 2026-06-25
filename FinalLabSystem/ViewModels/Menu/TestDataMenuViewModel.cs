using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class TestDataMenuViewModel : ViewModelBase
{
    public TestDataMenuViewModel(INavigationService navigationService)
    {
        NavigateToTestDataCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<TestDataManagementViewModel>());
        NavigateToCategoriesGroupsCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<CategoriesGroupsViewModel>());
    }

    public ICommand NavigateToTestDataCommand { get; }

    public ICommand NavigateToCategoriesGroupsCommand { get; }
}
