using System.Collections.ObjectModel;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class InventoryWindowViewModel : ViewModelBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IDialogService _dialogService;
    private int _lowStockCount;
    private string _statusMessage = string.Empty;

    public InventoryWindowViewModel(IInventoryService inventoryService, IDialogService dialogService)
    {
        _inventoryService = inventoryService;
        _dialogService = dialogService;

        Materials = new ObservableCollection<InventoryRowViewModel>();
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AdjustStockCommand = new AsyncRelayCommand<InventoryRowViewModel>(ExecuteAdjustStockAsync);
    }

    public ObservableCollection<InventoryRowViewModel> Materials { get; }

    public ICommand RefreshCommand { get; }

    public ICommand AdjustStockCommand { get; }

    public int LowStockCount
    {
        get => _lowStockCount;
        set => SetProperty(ref _lowStockCount, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            var materials = await _inventoryService.GetAllAsync();
            Materials.Clear();
            foreach (var m in materials)
                Materials.Add(new InventoryRowViewModel(m, AdjustStockCommand));

            LowStockCount = await _inventoryService.GetLowStockCountAsync();
            StatusMessage = LowStockCount > 0
                ? $"إجمالي: {Materials.Count} نوع — {LowStockCount} منخفض المخزون"
                : $"إجمالي: {Materials.Count} نوع — المخزون مقبول";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تحميل بيانات المخزون: {ex.Message}");
        }
    }

    private async Task ExecuteAdjustStockAsync(InventoryRowViewModel? row)
    {
        if (row is null) return;

        try
        {
            var currentStock = row.CurrentStock;
            var materialName = row.MaterialNameAr ?? row.MaterialName;

            var input = ShowAdjustmentInputDialog(materialName, currentStock);
            if (input is null) return;

            await _inventoryService.UpdateStockAsync(row.TubeMaterialId, input.Value.adjustment, input.Value.reason);
            row.CurrentStock = currentStock + input.Value.adjustment;
            LowStockCount = await _inventoryService.GetLowStockCountAsync();
            StatusMessage = $"تم تعديل مخزون '{materialName}' بنجاح";
        }
        catch (InvalidOperationException ex)
        {
            _dialogService.ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تعديل المخزون: {ex.Message}");
        }
    }

    private (int adjustment, string? reason)? ShowAdjustmentInputDialog(string materialName, int currentStock)
    {
        var dialog = new Views.Settings.StockAdjustmentDialog(materialName, currentStock)
        {
            Owner = System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().FirstOrDefault(w => w.IsActive)
        };

        if (dialog.ShowDialog() == true)
            return (dialog.Adjustment, dialog.Reason);

        return null;
    }
}
