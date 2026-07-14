using System.Windows.Input;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class SystemSettingsMenuViewModel
{
    public SystemSettingsMenuViewModel(
        ICommand navigateToTestDataCommand,
        ICommand navigateToCategoriesGroupsCommand,
        ICommand navigateToStaffManagementCommand)
    {
        NavigateToTestDataCommand = navigateToTestDataCommand;
        NavigateToCategoriesGroupsCommand = navigateToCategoriesGroupsCommand;
        NavigateToStaffManagementCommand = navigateToStaffManagementCommand;
    }

    public ICommand NavigateToTestDataCommand { get; }

    public ICommand NavigateToCategoriesGroupsCommand { get; }

    public ICommand NavigateToStaffManagementCommand { get; }
}
