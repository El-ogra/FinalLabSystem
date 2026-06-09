using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class GroupListViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private GroupRowViewModel? _selectedGroup;
    private int? _currentCategoryId;

    public GroupListViewModel(ITestCatalogService testCatalogService)
    {
        _testCatalogService = testCatalogService;
    }

    public event EventHandler<GroupRowViewModel?>? SelectedGroupChanged;

    public ObservableCollection<GroupRowViewModel> AllGroups { get; } = new();

    public GroupRowViewModel? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetProperty(ref _selectedGroup, value))
                SelectedGroupChanged?.Invoke(this, value);
        }
    }

    public async Task RefreshAsync(int categoryId)
    {
        _currentCategoryId = categoryId;
        var groups = await _testCatalogService.GetGroupsByCategoryIdAsync(categoryId);
        AllGroups.Clear();
        foreach (var row in groups.Select(g => new GroupRowViewModel(g)))
            AllGroups.Add(row);

        SelectedGroup = null;
    }

    public async Task RefreshCurrentCategoryAsync()
    {
        if (_currentCategoryId is null)
            return;

        await RefreshAsync(_currentCategoryId.Value);
    }

    public void Clear()
    {
        AllGroups.Clear();
        SelectedGroup = null;
        _currentCategoryId = null;
    }
}
