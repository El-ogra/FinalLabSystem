using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class PriceSchemeWindowViewModel : ViewModelBase
{
    private readonly IPricingService _pricingService;
    private readonly ITestCatalogService _testCatalogService;
    private PriceSchemeRowViewModel? _selectedScheme;
    private string _title = "إدارة أسعار التسعير";
    private bool _isEditing;
    private PriceSchemeRowViewModel _editModel = new();
    private ObservableCollection<PriceSchemeRowViewModel> _schemes = new();
    private ObservableCollection<TestTypePriceRowViewModel> _testPrices = new();
    private List<TestType> _allTestTypes = new();

    public PriceSchemeWindowViewModel(IPricingService pricingService, ITestCatalogService testCatalogService)
    {
        _pricingService = pricingService;
        _testCatalogService = testCatalogService;

        Schemes = new ObservableCollection<PriceSchemeRowViewModel>();
        TestPrices = new ObservableCollection<TestTypePriceRowViewModel>();

        AddCommand = new AsyncRelayCommand(ExecuteAddAsync);
        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, () => IsEditing);
        CancelCommand = new RelayCommand(_ => CancelEdit());
        RefreshCommand = new AsyncRelayCommand(ExecuteRefreshAsync);
        SavePricesCommand = new AsyncRelayCommand(ExecuteSavePricesAsync, () => SelectedScheme is not null);
    }

    public ObservableCollection<PriceSchemeRowViewModel> Schemes { get; }

    public ObservableCollection<TestTypePriceRowViewModel> TestPrices { get; }

    public PriceSchemeRowViewModel? SelectedScheme
    {
        get => _selectedScheme;
        set
        {
            if (SetProperty(ref _selectedScheme, value) && value is not null)
                _ = LoadPricesForSchemeAsync(value.SchemeId);
        }
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    public PriceSchemeRowViewModel EditModel
    {
        get => _editModel;
        private set => SetProperty(ref _editModel, value);
    }

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SavePricesCommand { get; }

    public async Task LoadAsync()
    {
        var schemes = await _pricingService.GetAllSchemesAsync();
        Schemes.Clear();
        foreach (var s in schemes)
        {
            Schemes.Add(new PriceSchemeRowViewModel
            {
                SchemeId = s.SchemeId,
                SchemeName = s.SchemeName,
                Description = s.Description,
                IsDefault = s.IsDefault,
                IsActive = s.IsActive
            });
        }

        _allTestTypes = await _testCatalogService.GetAllTestTypesAsync();
    }

    private async Task LoadPricesForSchemeAsync(int schemeId)
    {
        TestPrices.Clear();
        foreach (var testType in _allTestTypes.Where(t => t.IsActive))
        {
            var price = await _pricingService.GetTestPriceAsync(testType.TesttypeId, schemeId);
            TestPrices.Add(new TestTypePriceRowViewModel
            {
                TestTypeId = testType.TesttypeId,
                TypeCode = testType.TypeCode,
                TypeName = testType.TypeNameAr ?? testType.TypeNameEn,
                Price = price
            });
        }
    }

    private Task ExecuteAddAsync()
    {
        EditModel = new PriceSchemeRowViewModel { IsActive = true, IsDefault = false };
        IsEditing = true;
        Title = "إضافة مخطط تسعير جديد";
        return Task.CompletedTask;
    }

    private async Task ExecuteSaveAsync()
    {
        try
        {
            var model = new PriceScheme
            {
                SchemeId = EditModel.SchemeId,
                SchemeName = EditModel.SchemeName,
                Description = EditModel.Description,
                IsDefault = EditModel.IsDefault,
                IsActive = EditModel.IsActive
            };

            if (EditModel.SchemeId == 0)
            {
                var created = await _pricingService.CreateSchemeAsync(model);
                Schemes.Add(new PriceSchemeRowViewModel
                {
                    SchemeId = created.SchemeId,
                    SchemeName = created.SchemeName,
                    Description = created.Description,
                    IsDefault = created.IsDefault,
                    IsActive = created.IsActive
                });
            }
            else
            {
                await _pricingService.UpdateSchemeAsync(model);
                var existing = Schemes.FirstOrDefault(s => s.SchemeId == EditModel.SchemeId);
                if (existing is not null)
                {
                    existing.SchemeName = EditModel.SchemeName;
                    existing.Description = EditModel.Description;
                    existing.IsDefault = EditModel.IsDefault;
                    existing.IsActive = EditModel.IsActive;
                }
            }

            IsEditing = false;
            Title = "إدارة أسعار التسعير";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"خطأ في الحفظ: {ex.Message}", "خطأ", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        Title = "إدارة أسعار التسعير";
        EditModel = new PriceSchemeRowViewModel();
    }

    private async Task ExecuteRefreshAsync()
    {
        await LoadAsync();
    }

    private async Task ExecuteSavePricesAsync()
    {
        if (SelectedScheme is null) return;

        var prices = TestPrices
            .Where(tp => tp.Price > 0)
            .Select(tp => new TestTypePrice
            {
                SchemeId = SelectedScheme.SchemeId,
                TesttypeId = tp.TestTypeId,
                Price = tp.Price
            })
            .ToList();

        await _pricingService.UpdateSchemePricesAsync(SelectedScheme.SchemeId, prices);
        System.Windows.MessageBox.Show("تم حفظ الأسعار بنجاح", "تم", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}

public sealed class PriceSchemeRowViewModel : ViewModelBase
{
    private int _schemeId;
    private string _schemeName = string.Empty;
    private string? _description;
    private bool _isDefault;
    private bool _isActive;

    public int SchemeId
    {
        get => _schemeId;
        set => SetProperty(ref _schemeId, value);
    }

    public string SchemeName
    {
        get => _schemeName;
        set => SetProperty(ref _schemeName, value);
    }

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}

public sealed class TestTypePriceRowViewModel : ViewModelBase
{
    private int _testTypeId;
    private string _typeCode = string.Empty;
    private string _typeName = string.Empty;
    private decimal _price;

    public int TestTypeId
    {
        get => _testTypeId;
        set => SetProperty(ref _testTypeId, value);
    }

    public string TypeCode
    {
        get => _typeCode;
        set => SetProperty(ref _typeCode, value);
    }

    public string TypeName
    {
        get => _typeName;
        set => SetProperty(ref _typeName, value);
    }

    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }
}
