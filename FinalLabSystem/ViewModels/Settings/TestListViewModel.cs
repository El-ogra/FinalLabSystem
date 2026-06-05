using System.Collections.ObjectModel;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public enum SearchMode
{
    Code,
    Group,
    Name
}

public sealed class TestListViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private SearchMode _searchMode = SearchMode.Name;
    private string _searchText = string.Empty;
    private TestRowViewModel? _selectedTest;

    public TestListViewModel(ITestCatalogService testCatalogService)
    {
        _testCatalogService = testCatalogService;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public event EventHandler<TestRowViewModel?>? SelectedTestChanged;

    public ObservableCollection<TestRowViewModel> AllTests { get; } = new();

    public ObservableCollection<TestRowViewModel> FilteredTests { get; } = new();

    public Array SearchModes => Enum.GetValues(typeof(SearchMode));

    public SearchMode SearchMode
    {
        get => _searchMode;
        set
        {
            if (SetProperty(ref _searchMode, value))
                ApplyFilter();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
                ApplyFilter();
        }
    }

    public TestRowViewModel? SelectedTest
    {
        get => _selectedTest;
        set
        {
            if (SetProperty(ref _selectedTest, value))
                SelectedTestChanged?.Invoke(this, value);
        }
    }

    public ICommand RefreshCommand { get; }

    public async Task RefreshAsync()
    {
        var tests = await _testCatalogService.GetAllTestTypesAsync();
        AllTests.Clear();
        foreach (var row in tests.Select(t => new TestRowViewModel(t)))
            AllTests.Add(row);

        ApplyFilter();
    }

    public void ApplyFilter()
    {
        var query = SearchText.Trim();
        IEnumerable<TestRowViewModel> rows = string.IsNullOrWhiteSpace(query)
            ? AllTests
            : AllTests.Where(row => Matches(row, query)).ToList();

        FilteredTests.Clear();
        foreach (var row in rows)
            FilteredTests.Add(row);
    }

    private bool Matches(TestRowViewModel row, string query)
    {
        return SearchMode switch
        {
            SearchMode.Code => string.Equals(row.TypeCode, query, StringComparison.OrdinalIgnoreCase),
            SearchMode.Group => Contains(row.GroupNameAr, query) || Contains(row.GroupNameEn, query),
            SearchMode.Name => StartsWith(row.TypeNameAr, query) || StartsWith(row.TypeNameEn, query),
            _ => true
        };
    }

    private static bool Contains(string? source, string query)
        => !string.IsNullOrWhiteSpace(source)
           && source.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static bool StartsWith(string? source, string query)
        => !string.IsNullOrWhiteSpace(source)
           && source.StartsWith(query, StringComparison.OrdinalIgnoreCase);
}
