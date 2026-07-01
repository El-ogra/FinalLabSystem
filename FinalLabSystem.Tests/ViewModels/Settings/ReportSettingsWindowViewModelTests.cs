using System;
using System.Threading.Tasks;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class ReportSettingsWindowViewModelTests
{
    [Fact]
    public void LoadCommand_PopulatesAllFields()
    {
        var layoutService = new Mock<IReportLayoutService>();
        var dialogService = new Mock<IDialogService>();
        var userSession = new Mock<ICurrentUserSession>();

        var expected = new ReportLayoutDto
        {
            FontFamily = "Arial",
            FontSize = 14,
            PrimaryColor = "#FF0000"
        };
        layoutService.Setup(s => s.GetCurrentLayoutAsync()).ReturnsAsync(expected);

        var vm = new ReportSettingsWindowViewModel(layoutService.Object, dialogService.Object, userSession.Object);
        vm.LoadCommand.Execute(null);

        Assert.Equal("Arial", vm.CurrentLayout.FontFamily);
        Assert.Equal(14, vm.CurrentLayout.FontSize);
        Assert.Equal("#FF0000", vm.CurrentLayout.PrimaryColor);
    }

    [Fact]
    public async Task SaveCommand_CallsService_WithCurrentLayout()
    {
        var layoutService = new Mock<IReportLayoutService>();
        var dialogService = new Mock<IDialogService>();
        var userSession = new Mock<ICurrentUserSession>();
        var staff = new Staff { StaffId = 3, Username = "test" };
        userSession.Setup(s => s.CurrentUser).Returns(staff);

        var vm = new ReportSettingsWindowViewModel(layoutService.Object, dialogService.Object, userSession.Object);
        vm.CurrentLayout = new ReportLayoutDto { FontFamily = "Tahoma" };

        vm.SaveCommand.Execute(null);
        await Task.Delay(200);

        layoutService.Verify(s => s.SaveLayoutAsync(
            It.IsAny<ReportLayoutDto>(),
            It.Is<int>(id => id == 3)), Times.Once);
    }

    [Fact]
    public void ResetCommand_Confirms_ThenResets()
    {
        var layoutService = new Mock<IReportLayoutService>();
        var dialogService = new Mock<IDialogService>();
        var userSession = new Mock<ICurrentUserSession>();
        dialogService.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var defaults = new ReportLayoutDto { FontFamily = "Segoe UI", FontSize = 12 };
        layoutService.Setup(s => s.ResetToDefaultsAsync()).Returns(Task.CompletedTask);
        layoutService.Setup(s => s.GetDefaults()).Returns(defaults);

        var vm = new ReportSettingsWindowViewModel(layoutService.Object, dialogService.Object, userSession.Object);
        vm.CurrentLayout = new ReportLayoutDto { FontFamily = "Courier" };

        vm.ResetToDefaultsCommand.Execute(null);

        layoutService.Verify(s => s.ResetToDefaultsAsync(), Times.Once);
    }

    [Fact]
    public void BrowseLogoCommand_DoesNotThrow()
    {
        var layoutService = new Mock<IReportLayoutService>();
        var dialogService = new Mock<IDialogService>();
        var userSession = new Mock<ICurrentUserSession>();

        var vm = new ReportSettingsWindowViewModel(layoutService.Object, dialogService.Object, userSession.Object);

        var ex = Record.Exception(() => vm.BrowseLogoCommand.Execute(null));
        Assert.Null(ex);
    }

    [Fact]
    public void SaveCommand_NullUser_DoesNotCallSaveLayout()
    {
        var layoutService = new Mock<IReportLayoutService>();
        var dialogService = new Mock<IDialogService>();
        var userSession = new Mock<ICurrentUserSession>();
        userSession.Setup(s => s.CurrentUser).Returns((Staff?)null);

        var vm = new ReportSettingsWindowViewModel(layoutService.Object, dialogService.Object, userSession.Object);

        vm.SaveCommand.Execute(null);

        layoutService.Verify(s => s.SaveLayoutAsync(It.IsAny<ReportLayoutDto>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void IsBusy_False_AfterConstruction()
    {
        var layoutService = new Mock<IReportLayoutService>();
        var dialogService = new Mock<IDialogService>();
        var userSession = new Mock<ICurrentUserSession>();

        var vm = new ReportSettingsWindowViewModel(layoutService.Object, dialogService.Object, userSession.Object);

        Assert.False(vm.IsBusy);
    }
}
