using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Settings;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class ReportSettingsMenuViewModel : ViewModelBase
{
    public ReportSettingsMenuViewModel(INavigationService navigationService)
    {
        ManageTemplatesCommand = new RelayCommand(_ =>
            navigationService.OpenTaskWindow<ReportCommentTemplateViewModel>());

        NavigateToReportSettingsCommand = new RelayCommand(_ =>
            navigationService.OpenTaskWindow<ReportSettingsWindowViewModel>());
    }

    public ICommand ManageTemplatesCommand { get; }

    public ICommand NavigateToReportSettingsCommand { get; }
}
