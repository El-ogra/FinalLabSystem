using FinalLabSystem.Infrastructure;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Menu;

public sealed class HomeMenuViewModel : ViewModelBase
{
    private readonly IInventoryService _inventoryService;
    private int _lowStockCount;

    public HomeMenuViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
        WelcomeMessage = "مرحباً بكم في نظام FinalLab";
        _ = LoadAsync();
    }

    public string WelcomeMessage { get; }

    public int LowStockCount
    {
        get => _lowStockCount;
        private set => SetProperty(ref _lowStockCount, value);
    }

    private async Task LoadAsync()
    {
        try
        {
            LowStockCount = await _inventoryService.GetLowStockCountAsync();
        }
        catch
        {
            LowStockCount = 0;
        }
    }
}
