using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class CategoryListViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private CategoryRowViewModel? _selectedCategory;

    public CategoryListViewModel(ITestCatalogService testCatalogService)
    {
        _testCatalogService = testCatalogService;
    }

    public event EventHandler<CategoryRowViewModel?>? SelectedCategoryChanged;

    public ObservableCollection<CategoryRowViewModel> AllCategories { get; } = new();

    public CategoryRowViewModel? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
                SelectedCategoryChanged?.Invoke(this, value);
        }
    }

    public async Task RefreshAsync()
    {
        var categories = await _testCatalogService.GetFullHierarchyAsync();
        AllCategories.Clear();
        foreach (var row in categories.Select(c => new CategoryRowViewModel(c)))
            AllCategories.Add(row);

        SelectedCategory = null;
    }
}
