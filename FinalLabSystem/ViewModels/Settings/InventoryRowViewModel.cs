using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class InventoryRowViewModel : ViewModelBase
{
    private readonly TubeMaterial _material;
    private int _currentStock;

    public InventoryRowViewModel(TubeMaterial material, ICommand adjustStockCommand)
    {
        _material = material;
        _currentStock = material.CurrentStock;
        AdjustStockCommand = adjustStockCommand;
    }

    public int TubeMaterialId => _material.TubeMaterialId;
    public string MaterialName => _material.MaterialName;
    public string? MaterialNameAr => _material.MaterialNameAr;
    public string? TubeColor => _material.TubeColor;
    public int MinimumStock => _material.MinimumStock;
    public int SortOrder => _material.SortOrder;

    public int CurrentStock
    {
        get => _currentStock;
        set
        {
            if (SetProperty(ref _currentStock, value))
            {
                _material.CurrentStock = value;
                OnPropertyChanged(nameof(IsLowStock));
            }
        }
    }

    public bool IsLowStock => MinimumStock > 0 && CurrentStock <= MinimumStock;

    public ICommand AdjustStockCommand { get; }
}
