using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class NormalRangeWindowViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private TestType? _parentTest;
    private string _title = "القيم الطبيعية";

    public NormalRangeWindowViewModel(
        NormalRangeListViewModel list,
        NormalRangeDetailViewModel detail,
        ITestCatalogService testCatalogService)
    {
        List = list;
        Detail = detail;
        _testCatalogService = testCatalogService;
        SaveAllCommand = new AsyncRelayCommand(SaveAllAsync);
        CloseCommand = new RelayCommand(parameter =>
        {
            if (parameter is System.Windows.Window window)
                window.Close();
        });
    }

    public NormalRangeListViewModel List { get; }

    public NormalRangeDetailViewModel Detail { get; }

    public TestType? ParentTest
    {
        get => _parentTest;
        private set => SetProperty(ref _parentTest, value);
    }

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public ICommand SaveAllCommand { get; }

    public ICommand CloseCommand { get; }

    public async Task InitializeAsync(TestType parentTest)
    {
        ParentTest = await _testCatalogService.GetTestTypeDetailsAsync(parentTest.TesttypeId) ?? parentTest;
        Title = $"القيم الطبيعية - {ParentTest.TypeNameEn}";

        var components = ParentTest.TestComponents.ToList();
        if (components.Count == 0)
        {
            components.Add(new TestComponent
            {
                TesttypeId = ParentTest.TesttypeId,
                ComponentCode = ParentTest.TypeCode,
                ComponentNameEn = ParentTest.TypeNameEn,
                ResultType = "NUMERIC",
                IsActive = true,
                SortOrder = 1
            });
        }

        List.LoadComponents(components);
    }

    private async Task SaveAllAsync()
    {
        await List.SaveAllAsync();
    }
}
