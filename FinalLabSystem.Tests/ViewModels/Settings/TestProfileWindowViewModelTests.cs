using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels;

public class TestProfileWindowViewModelTests
{
    private static (TestProfileWindowViewModel vm, Mock<ITestCatalogService> mockCatalog,
        Mock<IDialogService> mockDialog)
        CreateViewModel()
    {
        var mockCatalog = new Mock<ITestCatalogService>();
        var mockSession = new Mock<ICurrentUserSession>();
        var mockDialog = new Mock<IDialogService>();

        mockCatalog.Setup(s => s.GetAllProfilesAsync())
            .ReturnsAsync(new List<TestProfile>());
        mockCatalog.Setup(s => s.GetAllTestTypesAsync())
            .ReturnsAsync(new List<TestType>());

        var staff = new FinalLabSystem.Models.Staff
        {
            StaffId = 1,
            Username = "testuser",
            DisplayName = "Test User"
        };
        mockSession.Setup(s => s.CurrentUser).Returns(staff);

        var vm = new TestProfileWindowViewModel(
            mockCatalog.Object,
            mockSession.Object,
            mockDialog.Object);

        return (vm, mockCatalog, mockDialog);
    }

    [Fact]
    public void Commands_AreInitialized()
    {
        var (vm, _, _) = CreateViewModel();

        Assert.NotNull(vm.NewProfileCommand);
        Assert.NotNull(vm.SaveProfileCommand);
        Assert.NotNull(vm.DeleteProfileCommand);
        Assert.NotNull(vm.AddTestCommand);
        Assert.NotNull(vm.RemoveTestCommand);
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
    public void NewProfileCommand_SetsIsEditingTrue()
    {
        var (vm, _, _) = CreateViewModel();

        vm.NewProfileCommand.Execute(null);

        Assert.True(vm.IsEditing);
        Assert.Equal(string.Empty, vm.EditableProfileNameAr);
    }

    [Fact]
    public void DeleteProfile_WithNoSelection_DoesNothing()
    {
        var (vm, _, mockDialog) = CreateViewModel();

        vm.DeleteProfileCommand.Execute(null);

        mockDialog.Verify(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void DeleteProfile_WhenDeclined_DoesNotDelete()
    {
        var (vm, mockCatalog, mockDialog) = CreateViewModel();

        var profile = new TestProfile
        {
            ProfileId = 1,
            ProfileNameAr = "Test",
            ProfileNameEn = "Test",
            IsActive = true
        };
        mockCatalog.Setup(s => s.GetAllProfilesAsync())
            .ReturnsAsync(new List<TestProfile> { profile });
        mockCatalog.Setup(s => s.GetAllTestTypesAsync())
            .ReturnsAsync(new List<TestType>());

        vm.SelectedProfile = new TestProfileRowViewModel(profile);

        mockDialog.Setup(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        vm.DeleteProfileCommand.Execute(null);

        mockDialog.Verify(d => d.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
