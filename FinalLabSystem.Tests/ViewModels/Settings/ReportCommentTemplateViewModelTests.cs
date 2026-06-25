using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class ReportCommentTemplateViewModelTests
{
    private static (ReportCommentTemplateViewModel vm, Mock<IReportCommentTemplateService> mockTemplate,
        Mock<IDialogService> mockDialog)
        CreateViewModel()
    {
        var mockTemplate = new Mock<IReportCommentTemplateService>();
        var mockCatalog = new Mock<ITestCatalogService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();

        mockTemplate.Setup(s => s.GetActiveTemplatesAsync())
            .ReturnsAsync(new List<ReportCommentTemplate>());
        mockCatalog.Setup(s => s.GetFullHierarchyAsync())
            .ReturnsAsync(new List<TestCategory>());

        var staff = new FinalLabSystem.Models.Staff
        {
            StaffId = 1,
            Username = "testuser",
            DisplayName = "Test User"
        };
        mockSession.Setup(s => s.CurrentUser).Returns(staff);

        var vm = new ReportCommentTemplateViewModel(
            mockTemplate.Object,
            mockCatalog.Object,
            mockSession.Object,
            mockDialog.Object);

        return (vm, mockTemplate, mockDialog);
    }

    [Fact]
    public void Commands_AreInitialized()
    {
        var (vm, _, _) = CreateViewModel();

        Assert.NotNull(vm.NewCommand);
        Assert.NotNull(vm.SaveCommand);
        Assert.NotNull(vm.DeleteCommand);
        Assert.NotNull(vm.RefreshCommand);
        Assert.NotNull(vm.CloseCommand);
    }

    [Fact]
    public void IsEditing_DefaultsToFalse()
    {
        var (vm, _, _) = CreateViewModel();

        Assert.False(vm.IsEditing);
    }

    [Fact]
    public void NewCommand_SetsIsEditingTrue()
    {
        var (vm, _, _) = CreateViewModel();

        vm.NewCommand.Execute(null);

        Assert.True(vm.IsEditing);
        Assert.Equal(string.Empty, vm.EditableTitle);
        Assert.Equal("AR", vm.EditableCommentLang);
        Assert.Equal("Manual", vm.EditableTriggerCondition);
    }

    [Fact]
    public void AvailableTriggers_ContainsExpectedValues()
    {
        var (vm, _, _) = CreateViewModel();

        Assert.Contains("None", vm.AvailableTriggers);
        Assert.Contains("Low", vm.AvailableTriggers);
        Assert.Contains("High", vm.AvailableTriggers);
        Assert.Contains("Critical", vm.AvailableTriggers);
        Assert.Contains("Manual", vm.AvailableTriggers);
    }

    [Fact]
    public void AvailableLanguages_ContainsArabicAndEnglish()
    {
        var (vm, _, _) = CreateViewModel();

        Assert.Contains("AR", vm.AvailableLanguages);
        Assert.Contains("EN", vm.AvailableLanguages);
    }

    [Fact]
    public void DeleteTemplate_WhenDeclined_DoesNotDelete()
    {
        var (vm, _, mockDialog) = CreateViewModel();

        mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        vm.DeleteCommand.Execute(null);

        mockDialog.Verify(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void DeleteTemplate_WithNoSelection_DoesNothing()
    {
        var (vm, _, mockDialog) = CreateViewModel();

        vm.DeleteCommand.Execute(null);

        mockDialog.Verify(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
