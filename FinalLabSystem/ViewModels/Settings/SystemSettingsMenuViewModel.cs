using System.Windows.Input;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class SystemSettingsMenuViewModel
{
    public SystemSettingsMenuViewModel(ICommand navigateToTestDataCommand)
    {
        NavigateToTestDataCommand = navigateToTestDataCommand;
    }

    public ICommand NavigateToTestDataCommand { get; }
}
