using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.ViewModels.Menu;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Menu;

public class ReportSettingsMenuViewModelTests
{
    [Fact]
    public void NavigateToReportSettingsCommand_CallsNavigationService_OpenTaskWindow()
    {
        var navService = new Mock<INavigationService>();
        var vm = new ReportSettingsMenuViewModel(navService.Object);

        vm.NavigateToReportSettingsCommand.Execute(null);

        navService.Verify(n => n.OpenTaskWindow<ReportSettingsWindowViewModel>(), Times.Once);
    }

    [Fact]
    public void ManageTemplatesCommand_StillWorks_AfterAddingNavigateCommand()
    {
        var navService = new Mock<INavigationService>();
        var vm = new ReportSettingsMenuViewModel(navService.Object);

        vm.ManageTemplatesCommand.Execute(null);

        navService.Verify(n => n.OpenTaskWindow<ReportCommentTemplateViewModel>(), Times.Once);
    }
}
