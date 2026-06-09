using System.Collections.ObjectModel;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestListViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private string _searchTestName = string.Empty;
    private string _searchGroupName = string.Empty;
    private string _searchTestId = string.Empty;
    private TestRowViewModel? _selectedTest;
    private ICommand? _searchCommand;

    public TestListViewModel(ITestCatalogService testCatalogService)
    {
        _testCatalogService = testCatalogService;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    public event EventHandler<TestRowViewModel?>? SelectedTestChanged;

    public ObservableCollection<TestRowViewModel> AllTests { get; } = new();

    public ObservableCollection<TestRowViewModel> FilteredTests { get; } = new();

    public string SearchTestName
    {
        get => _searchTestName;
        set
        {
            if (SetProperty(ref _searchTestName, value ?? string.Empty))
                ApplyFilter();
        }
    }

    public string SearchGroupName
    {
        get => _searchGroupName;
        set
        {
            if (SetProperty(ref _searchGroupName, value ?? string.Empty))
                ApplyFilter();
        }
    }

    public string SearchTestId
    {
        get => _searchTestId;
        set
        {
            if (SetProperty(ref _searchTestId, value ?? string.Empty))
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

    public ICommand SearchCommand => _searchCommand ??= new RelayCommand(_ => ApplyFilter());

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
        IEnumerable<TestRowViewModel> rows = AllTests;

        if (!string.IsNullOrWhiteSpace(SearchTestName))
            rows = rows.Where(row => row.TypeNameEn.Contains(SearchTestName, StringComparison.OrdinalIgnoreCase)
                                  || (row.TypeNameAr?.Contains(SearchTestName, StringComparison.OrdinalIgnoreCase) ?? false));

        if (!string.IsNullOrWhiteSpace(SearchGroupName))
            rows = rows.Where(row => row.GroupNameEn.Contains(SearchGroupName, StringComparison.OrdinalIgnoreCase)
                                  || row.GroupNameAr.Contains(SearchGroupName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(SearchTestId))
            rows = rows.Where(row => row.TypeCode.Contains(SearchTestId, StringComparison.OrdinalIgnoreCase));

        FilteredTests.Clear();
        foreach (var row in rows)
            FilteredTests.Add(row);
    }
}
