using System.Windows.Input;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class SystemSettingsMenuViewModel
{
    public SystemSettingsMenuViewModel(
        ICommand navigateToTestDataCommand,
        ICommand navigateToCategoriesGroupsCommand,
        ICommand navigateToStaffManagementCommand,
        ICommand navigateToSecuritySettingsCommand)
    {
        NavigateToTestDataCommand = navigateToTestDataCommand;
        NavigateToCategoriesGroupsCommand = navigateToCategoriesGroupsCommand;
        NavigateToStaffManagementCommand = navigateToStaffManagementCommand;
        NavigateToSecuritySettingsCommand = navigateToSecuritySettingsCommand;
    }

    public ICommand NavigateToTestDataCommand { get; }

    public ICommand NavigateToCategoriesGroupsCommand { get; }

    public ICommand NavigateToStaffManagementCommand { get; }

    public ICommand NavigateToSecuritySettingsCommand { get; }
}
