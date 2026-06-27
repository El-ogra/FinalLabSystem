using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class InventoryWindowViewModelTests
{
    private static (InventoryWindowViewModel VM, Mock<IInventoryService> InventoryMock, Mock<IDialogService> DialogMock) CreateVM()
    {
        var inventoryMock = new Mock<IInventoryService>();
        var dialogMock = new Mock<IDialogService>();
        var vm = new InventoryWindowViewModel(inventoryMock.Object, dialogMock.Object);
        return (vm, inventoryMock, dialogMock);
    }

    [Fact]
    public async Task LoadAsync_PopulatesMaterials()
    {
        var (vm, inventoryMock, _) = CreateVM();
        inventoryMock.Setup(i => i.GetAllAsync())
            .ReturnsAsync(new List<TubeMaterial>
            {
                new() { TubeMaterialId = 1, MaterialName = "Red Top", MaterialNameAr = "أنابيب حمراء", CurrentStock = 50, MinimumStock = 10, IsActive = true },
                new() { TubeMaterialId = 2, MaterialName = "EDTA", MaterialNameAr = "EDTA", CurrentStock = 30, MinimumStock = 5, IsActive = true }
            });
        inventoryMock.Setup(i => i.GetLowStockCountAsync()).ReturnsAsync(0);

        await vm.LoadAsync();

        Assert.Equal(2, vm.Materials.Count);
    }

    [Fact]
    public async Task LoadAsync_SetsLowStockCount()
    {
        var (vm, inventoryMock, _) = CreateVM();
        inventoryMock.Setup(i => i.GetAllAsync())
            .ReturnsAsync(new List<TubeMaterial>
            {
                new() { TubeMaterialId = 1, MaterialName = "Red Top", CurrentStock = 5, MinimumStock = 10, IsActive = true }
            });
        inventoryMock.Setup(i => i.GetLowStockCountAsync()).ReturnsAsync(1);

        await vm.LoadAsync();

        Assert.Equal(1, vm.LowStockCount);
    }

    [Fact]
    public async Task RefreshCommand_ReloadsData()
    {
        var (vm, inventoryMock, _) = CreateVM();
        inventoryMock.Setup(i => i.GetAllAsync())
            .ReturnsAsync(new List<TubeMaterial>
            {
                new() { TubeMaterialId = 1, MaterialName = "Red Top", CurrentStock = 50, MinimumStock = 10, IsActive = true }
            });
        inventoryMock.Setup(i => i.GetLowStockCountAsync()).ReturnsAsync(0);

        await vm.LoadAsync();
        Assert.Single(vm.Materials);

        inventoryMock.Setup(i => i.GetAllAsync())
            .ReturnsAsync(new List<TubeMaterial>
            {
                new() { TubeMaterialId = 1, MaterialName = "Red Top", CurrentStock = 50, MinimumStock = 10, IsActive = true },
                new() { TubeMaterialId = 2, MaterialName = "EDTA", CurrentStock = 30, MinimumStock = 5, IsActive = true }
            });

        vm.RefreshCommand.Execute(null);
        await Task.Delay(100);

        Assert.Equal(2, vm.Materials.Count);
    }

    [Fact]
    public async Task LoadAsync_ZeroLowStock_SetsStatusContainsمقبول()
    {
        var (vm, inventoryMock, _) = CreateVM();
        inventoryMock.Setup(i => i.GetAllAsync())
            .ReturnsAsync(new List<TubeMaterial>
            {
                new() { TubeMaterialId = 1, MaterialName = "Red Top", CurrentStock = 50, MinimumStock = 10, IsActive = true }
            });
        inventoryMock.Setup(i => i.GetLowStockCountAsync()).ReturnsAsync(0);

        await vm.LoadAsync();

        Assert.Contains("مقبول", vm.StatusMessage);
    }

    [Fact]
    public async Task LoadAsync_PositiveLowStock_SetsStatusContainsمنخفض()
    {
        var (vm, inventoryMock, _) = CreateVM();
        inventoryMock.Setup(i => i.GetAllAsync())
            .ReturnsAsync(new List<TubeMaterial>
            {
                new() { TubeMaterialId = 1, MaterialName = "Red Top", CurrentStock = 5, MinimumStock = 10, IsActive = true }
            });
        inventoryMock.Setup(i => i.GetLowStockCountAsync()).ReturnsAsync(1);

        await vm.LoadAsync();

        Assert.Contains("منخفض", vm.StatusMessage);
    }

    [Fact]
    public void Materials_IsEmpty_WhenNoData()
    {
        var (vm, _, _) = CreateVM();

        Assert.Empty(vm.Materials);
    }
}
