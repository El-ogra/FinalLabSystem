using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Menu;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Menu;

public class HomeMenuViewModelLowStockTests
{
    private static (HomeMenuViewModel VM, Mock<IInventoryService> InventoryMock) CreateVM(int lowStockCount = 0)
    {
        var inventoryMock = new Mock<IInventoryService>();
        inventoryMock.Setup(i => i.GetLowStockCountAsync()).ReturnsAsync(lowStockCount);
        var vm = new HomeMenuViewModel(inventoryMock.Object);
        return (vm, inventoryMock);
    }

    [Fact]
    public void Constructor_SetsWelcomeMessage()
    {
        var (vm, _) = CreateVM();

        Assert.Equal("مرحباً بكم في نظام FinalLab", vm.WelcomeMessage);
    }

    [Fact]
    public async Task LoadAsync_SetsLowStockCount()
    {
        var (vm, _) = CreateVM(lowStockCount: 3);

        await Task.Delay(100);

        Assert.Equal(3, vm.LowStockCount);
    }

    [Fact]
    public void InventoryService_CalledOnConstruction()
    {
        var (vm, inventoryMock) = CreateVM(lowStockCount: 2);

        inventoryMock.Verify(i => i.GetLowStockCountAsync(), Times.Once);
    }

    [Fact]
    public async Task LowStockCount_Zero_AfterLoad_WithNoLowStock()
    {
        var (vm, _) = CreateVM(lowStockCount: 0);

        await Task.Delay(100);

        Assert.Equal(0, vm.LowStockCount);
    }
}
