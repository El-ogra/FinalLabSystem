using System.Windows.Input;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class SystemSettingsMenuViewModel
{
    public SystemSettingsMenuViewModel(ICommand navigateToTestDataCommand, ICommand navigateToCategoriesGroupsCommand)
    {
        NavigateToTestDataCommand = navigateToTestDataCommand;
        NavigateToCategoriesGroupsCommand = navigateToCategoriesGroupsCommand;
    }

    public ICommand NavigateToTestDataCommand { get; }

    public ICommand NavigateToCategoriesGroupsCommand { get; }
}
