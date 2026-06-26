using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Patients;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class TestSelectionViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly ITestCatalogService _testCatalogService;
    private readonly TestPricingEngine _pricingEngine;
    private readonly List<TestDisplayItem> _allTests = new();
    private string _activeFilter = "RoutineTests";
    private string? _searchText;
    private TestDisplayItem? _selectedAvailableTest;
    private SelectedTestItem? _selectedTest;
    private int? _schemeId;

    public TestSelectionViewModel(ITestCatalogService testCatalogService, TestPricingEngine pricingEngine)
    {
        _testCatalogService = testCatalogService;
        _pricingEngine = pricingEngine;
        AvailableTests = new ObservableCollection<TestDisplayItem>();
        SelectedTests = new ObservableCollection<SelectedTestItem>();
        Filters = new ObservableCollection<string> { "RoutineTests", "Profiles", "ByGroup", "ByCategory" };
        AddTestCommand = new RelayCommand(parameter => AddTest(parameter as TestDisplayItem ?? SelectedAvailableTest));
        RemoveTestCommand = new RelayCommand(parameter => RemoveTest(parameter as SelectedTestItem ?? SelectedTest));
        RemoveAllCommand = new RelayCommand(_ => RemoveAll());
        SetFilterCommand = new RelayCommand(parameter => ActiveFilter = parameter?.ToString() ?? "RoutineTests");
        ApplyProfileCommand = new AsyncRelayCommand(ApplyProfileAsync);
    }

    public int? SchemeId
    {
        get => _schemeId;
        set => SetProperty(ref _schemeId, value);
    }

    public async Task InitializeAsync()
    {
        try
        {
            var categories = await _testCatalogService.GetFullHierarchyAsync();
            _allTests.Clear();

            var allTestTypeIds = new List<int>();
            foreach (var category in categories)
            {
                foreach (var group in category.TestGroups.Where(group => group.IsActive))
                {
                    foreach (var test in group.TestTypes.Where(test => test.IsActive))
                    {
                        allTestTypeIds.Add(test.TesttypeId);
                    }
                }
            }

            var pricingResults = await _pricingEngine.GetPricingSummaryAsync(allTestTypeIds, SchemeId);
            var pricingLookup = pricingResults.ToDictionary(r => r.TestTypeId);

            foreach (var category in categories)
            {
                foreach (var group in category.TestGroups.Where(group => group.IsActive))
                {
                    foreach (var test in group.TestTypes.Where(test => test.IsActive))
                    {
                        var pricing = pricingLookup[test.TesttypeId];
                        _allTests.Add(new TestDisplayItem(
                            test.TesttypeId,
                            test.TypeCode,
                            test.TypeNameAr ?? test.TypeNameEn,
                            test.TypeNameEn,
                            pricing.Price,
                            test.SampleType,
                            group.GroupNameAr ?? group.GroupNameEn,
                            category.CategoryNameAr ?? category.CategoryNameEn,
                            "RoutineTests",
                            pricing.IsFallback));
                    }
                }
            }

            var profiles = await _testCatalogService.GetActiveProfilesAsync();
            foreach (var profile in profiles)
            {
                var profileTests = await _testCatalogService.GetProfileTestsAsync(profile.ProfileId);
                var profileTestIds = profileTests.Where(test => test.IsActive).Select(t => t.TesttypeId).ToList();
                var profilePricing = await _pricingEngine.GetPricingSummaryAsync(profileTestIds, SchemeId);
                var profilePricingLookup = profilePricing.ToDictionary(r => r.TestTypeId);

                foreach (var test in profileTests.Where(test => test.IsActive))
                {
                    var pricing = profilePricingLookup[test.TesttypeId];
                    _allTests.Add(new TestDisplayItem(
                        test.TesttypeId,
                        test.TypeCode,
                        $"{profile.ProfileNameAr ?? profile.ProfileNameEn} - {test.TypeNameAr ?? test.TypeNameEn}",
                        test.TypeNameEn,
                        pricing.Price,
                        test.SampleType,
                        "Profiles",
                        "Profiles",
                        "Profiles",
                        pricing.IsFallback));
                }
            }

            ApplyFilter();
        }
        catch
        {
            // TODO F-07: _dialogService.ShowError("حدث خطأ أثناء تحميل البيانات.");
        }
    }

    public event EventHandler? TestsChanged;

    public IReadOnlyList<TestDisplayItem> AllTests => _allTests;

    public ObservableCollection<TestDisplayItem> AvailableTests { get; }

    public ObservableCollection<SelectedTestItem> SelectedTests { get; }

    public ObservableCollection<string> Filters { get; }

    public int SelectedTestsCount => SelectedTests.Count;

    public string ActiveFilter
    {
        get => _activeFilter;
        set
        {
            if (SetProperty(ref _activeFilter, value))
                ApplyFilter();
        }
    }

    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilter();
        }
    }

    public TestDisplayItem? SelectedAvailableTest
    {
        get => _selectedAvailableTest;
        set => SetProperty(ref _selectedAvailableTest, value);
    }

    public SelectedTestItem? SelectedTest
    {
        get => _selectedTest;
        set => SetProperty(ref _selectedTest, value);
    }

    public ICommand AddTestCommand { get; }

    public ICommand RemoveTestCommand { get; }

    public ICommand RemoveAllCommand { get; }

    public ICommand SetFilterCommand { get; }

    public ICommand ApplyProfileCommand { get; }

    public List<int> GetSelectedTestTypeIds() => SelectedTests.Select(test => test.TestTypeId).Distinct().ToList();

    public List<decimal> GetSelectedPrices() => SelectedTests.Select(test => test.Price).ToList();

    public void LoadSelectedTests(IEnumerable<VisitTest> visitTests)
    {
        SelectedTests.Clear();
        foreach (var visitTest in visitTests)
        {
            var testType = visitTest.Testtype;
            SelectedTests.Add(new SelectedTestItem(
                testType.TesttypeId,
                testType.TypeCode,
                testType.TypeNameAr ?? testType.TypeNameEn,
                Convert.ToDecimal(visitTest.PriceCharged),
                testType.SampleType,
                false));
        }

        OnPropertyChanged(nameof(SelectedTestsCount));
        TestsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadSelectedTests(IEnumerable<SelectedTestDto> selectedTests)
    {
        SelectedTests.Clear();
        foreach (var test in selectedTests)
        {
            SelectedTests.Add(new SelectedTestItem(
                test.TestTypeId,
                test.TestCode,
                test.TestName,
                test.Price,
                test.SampleType,
                false));
        }

        OnPropertyChanged(nameof(SelectedTestsCount));
        TestsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearAll()
    {
        SelectedTests.Clear();
        SelectedAvailableTest = null;
        SelectedTest = null;
        SearchText = null;
        ActiveFilter = "RoutineTests";
        OnPropertyChanged(nameof(SelectedTestsCount));
        TestsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyFilter()
    {
        var query = _allTests.AsEnumerable();
        if (ActiveFilter == "Profiles")
            query = query.Where(test => test.FilterKind == "Profiles");
        else if (ActiveFilter == "ByGroup")
            query = query.OrderBy(test => test.GroupName);
        else if (ActiveFilter == "ByCategory")
            query = query.OrderBy(test => test.CategoryName).ThenBy(test => test.GroupName);
        else
            query = query.Where(test => test.FilterKind == "RoutineTests");

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(test =>
                test.Code.Contains(term, StringComparison.OrdinalIgnoreCase)
                || test.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || test.NameEn.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        AvailableTests.Clear();
        foreach (var test in query.Take(200))
            AvailableTests.Add(test);
    }

    private void AddTest(TestDisplayItem? test)
    {
        if (test is null || SelectedTests.Any(selected => selected.TestTypeId == test.TestTypeId))
            return;

        SelectedTests.Add(new SelectedTestItem(test.TestTypeId, test.Code, test.Name, test.Price, test.SampleType, test.IsFallback));
        OnPropertyChanged(nameof(SelectedTestsCount));
        TestsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RemoveTest(SelectedTestItem? test)
    {
        if (test is null)
            return;

        SelectedTests.Remove(test);
        OnPropertyChanged(nameof(SelectedTestsCount));
        TestsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RemoveAll()
    {
        SelectedTests.Clear();
        OnPropertyChanged(nameof(SelectedTestsCount));
        TestsChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task ApplyProfileAsync()
    {
        var profiles = await _testCatalogService.GetActiveProfilesAsync();
        if (profiles.Count == 0)
        {
            MessageBox.Show("لا توجد بروفايلات نشطة", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new ProfileSelectionDialog();
        dialog.LoadProfiles(profiles);
        dialog.Owner = Application.Current.MainWindow;

        if (dialog.ShowDialog() != true || dialog.SelectedProfile == null)
            return;

        var profileTests = await _testCatalogService.GetProfileTestsAsync(dialog.SelectedProfile.ProfileId);

        int addedCount = 0;
        foreach (var test in profileTests.Where(test => test.IsActive))
        {
            if (SelectedTests.Any(s => s.TestTypeId == test.TesttypeId))
                continue;

            var pricing = await _pricingEngine.ResolvePriceAsync(test.TesttypeId, SchemeId);
            SelectedTests.Add(new SelectedTestItem(
                test.TesttypeId,
                test.TypeCode,
                test.TypeNameAr ?? test.TypeNameEn,
                pricing.Price,
                test.SampleType,
                pricing.IsFallback));
            addedCount++;
        }

        OnPropertyChanged(nameof(SelectedTestsCount));
        TestsChanged?.Invoke(this, EventArgs.Empty);

        if (addedCount > 0)
            MessageBox.Show($"تم إضافة {addedCount} تحليل من البروفايل", "تم", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show("جميع تحاليل البروفايل مضافة مسبقاً", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

public sealed record TestDisplayItem(
    int TestTypeId,
    string Code,
    string Name,
    string NameEn,
    decimal Price,
    string? SampleType,
    string GroupName,
    string CategoryName,
    string FilterKind,
    bool IsFallback = false);

public sealed record SelectedTestItem(int TestTypeId, string Code, string Name, decimal Price, string? SampleType, bool IsFallback = false);
