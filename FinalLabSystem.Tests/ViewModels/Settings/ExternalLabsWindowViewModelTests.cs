using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class ExternalLabsWindowViewModelTests
{
    private static (ExternalLabsWindowViewModel VM, Mock<IExternalLabRegistryService> LabMock, Mock<IExternalShipmentService> ShipMock) CreateVM()
    {
        var labMock = new Mock<IExternalLabRegistryService>();
        var shipMock = new Mock<IExternalShipmentService>();
        var vm = new ExternalLabsWindowViewModel(labMock.Object, shipMock.Object);
        return (vm, labMock, shipMock);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithoutError()
    {
        var (vm, _, _) = CreateVM();

        Assert.NotNull(vm);
        Assert.NotNull(vm.Labs);
        Assert.NotNull(vm.Shipments);
        Assert.NotNull(vm.PendingItems);
    }

    [Fact]
    public void Commands_ShouldBeBound_WhenCreated()
    {
        var (vm, _, _) = CreateVM();

        Assert.NotNull(vm.RefreshLabsCommand);
        Assert.NotNull(vm.CreateLabCommand);
        Assert.NotNull(vm.DeleteLabCommand);
        Assert.NotNull(vm.SendManifestCommand);
        Assert.NotNull(vm.ReceiveResultsCommand);
    }

    [Fact]
    public async Task InitializeAsync_ShouldPopulateLabs()
    {
        var (vm, labMock, _) = CreateVM();
        labMock.Setup(l => l.GetAllAsync()).ReturnsAsync(new List<ExternalLab>
        {
            new() { ExternalLabId = 1, LabName = "Lab A", IsActive = true },
            new() { ExternalLabId = 2, LabName = "Lab B", IsActive = true }
        });

        await vm.InitializeAsync();

        Assert.Equal(2, vm.Labs.Count);
    }

    [Fact]
    public async Task CreateLabCommand_ShouldCallCreateAndRefresh()
    {
        var (vm, labMock, _) = CreateVM();
        labMock.Setup(l => l.GetAllAsync()).ReturnsAsync(new List<ExternalLab>());
        vm.EditLabName = "New Lab";

        vm.CreateLabCommand.Execute(null);

        labMock.Verify(l => l.CreateAsync(It.IsAny<ExternalLab>()), Times.Once);
    }

    [Fact]
    public void CanCreateLab_ShouldBeFalse_WhenNameEmpty()
    {
        var (vm, _, _) = CreateVM();
        vm.EditLabName = string.Empty;

        Assert.False(vm.CanCreateLab);
    }

    [Fact]
    public void CanCreateLab_ShouldBeTrue_WhenNameProvided()
    {
        var (vm, _, _) = CreateVM();
        vm.EditLabName = "Lab X";

        Assert.True(vm.CanCreateLab);
    }
}
