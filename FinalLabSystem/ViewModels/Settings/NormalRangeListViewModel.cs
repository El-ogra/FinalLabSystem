using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class NormalRangeListViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private readonly NormalRangeDetailViewModel _detail;
    private TestComponent? _selectedComponent;
    private NormalRange? _selectedRange;

    public NormalRangeListViewModel(ITestCatalogService testCatalogService, NormalRangeDetailViewModel detail)
    {
        _testCatalogService = testCatalogService;
        _detail = detail;
        _detail.RangeApplied += OnRangeApplied;
        AddComponentCommand = new RelayCommand(_ => AddComponent());
        DeleteComponentCommand = new AsyncRelayCommand(DeleteComponentAsync, () => SelectedComponent is not null);
        AddRangeCommand = new RelayCommand(_ => AddRange(), _ => SelectedComponent is not null);
        EditRangeCommand = new RelayCommand(_ => EditRange(), _ => SelectedRange is not null);
        DeleteRangeCommand = new AsyncRelayCommand(DeleteRangeAsync, () => SelectedRange is not null);
    }

    public ObservableCollection<TestComponent> Components { get; } = new();

    public ObservableCollection<NormalRange> RangesForSelectedComponent { get; } = new();

    public TestComponent? SelectedComponent
    {
        get => _selectedComponent;
        set
        {
            if (SetProperty(ref _selectedComponent, value))
                _ = RefreshRangesAsync();
        }
    }

    public NormalRange? SelectedRange
    {
        get => _selectedRange;
        set
        {
            if (SetProperty(ref _selectedRange, value))
                _detail.Load(value, SelectedComponent?.Unit);
        }
    }

    public ICommand AddComponentCommand { get; }

    public ICommand DeleteComponentCommand { get; }

    public ICommand AddRangeCommand { get; }

    public ICommand EditRangeCommand { get; }

    public ICommand DeleteRangeCommand { get; }

    public void LoadComponents(IEnumerable<TestComponent> components)
    {
        Components.Clear();
        foreach (var component in components.OrderBy(c => c.SortOrder))
            Components.Add(CloneComponent(component));

        SelectedComponent = Components.FirstOrDefault();
    }

    public async Task SaveAllAsync()
    {
        foreach (var component in Components)
        {
            if (component.ComponentId == 0)
            {
                component.ComponentId = await _testCatalogService.AddComponentAsync(component.TesttypeId, component);
            }
            else
            {
                await _testCatalogService.UpdateComponentAsync(component);
            }
        }

        foreach (var range in RangesForSelectedComponent)
        {
            if (SelectedComponent is not null)
                range.ComponentId = SelectedComponent.ComponentId;

            if (range.RangeId == 0)
                range.RangeId = await _testCatalogService.AddRangeAsync(range);
            else
                await _testCatalogService.UpdateRangeAsync(range);
        }
    }

    private void AddComponent()
    {
        var next = Components.Count == 0 ? (short)1 : (short)(Components.Max(c => c.SortOrder) + 1);
        var component = new TestComponent
        {
            ComponentCode = "COMP" + next,
            ComponentNameEn = "New Component",
            ResultType = "NUMERIC",
            IsActive = true,
            SortOrder = next
        };
        Components.Add(component);
        SelectedComponent = component;
    }

    private async Task DeleteComponentAsync()
    {
        if (SelectedComponent is null)
            return;

        if (SelectedComponent.ComponentId > 0)
            await _testCatalogService.DeleteComponentAsync(SelectedComponent.ComponentId);

        Components.Remove(SelectedComponent);
        SelectedComponent = Components.FirstOrDefault();
    }

    private void AddRange()
    {
        if (SelectedComponent is null)
            return;

        var range = new NormalRange
        {
            ComponentId = SelectedComponent.ComponentId,
            Sex = "Both",
            FastingState = "Any",
            AgeFromDays = 0,
            AgeToDays = 36500
        };
        RangesForSelectedComponent.Add(range);
        SelectedRange = range;
    }

    private void EditRange()
    {
        if (SelectedRange is not null)
            _detail.Load(SelectedRange, SelectedComponent?.Unit);
    }

    private async Task DeleteRangeAsync()
    {
        if (SelectedRange is null)
            return;

        if (SelectedRange.RangeId > 0)
            await _testCatalogService.DeleteRangeAsync(SelectedRange.RangeId);

        RangesForSelectedComponent.Remove(SelectedRange);
        SelectedRange = RangesForSelectedComponent.FirstOrDefault();
    }

    private async Task RefreshRangesAsync()
    {
        RangesForSelectedComponent.Clear();
        if (SelectedComponent is null)
            return;

        if (SelectedComponent.ComponentId > 0)
        {
            foreach (var range in await _testCatalogService.GetRangesForComponentAsync(SelectedComponent.ComponentId))
                RangesForSelectedComponent.Add(range);
        }
        else
        {
            foreach (var range in SelectedComponent.NormalRanges)
                RangesForSelectedComponent.Add(range);
        }

        SelectedRange = RangesForSelectedComponent.FirstOrDefault();
        _detail.Unit = SelectedComponent.Unit;
    }

    private void OnRangeApplied(object? sender, NormalRange applied)
    {
        if (SelectedComponent is null)
            return;

        SelectedComponent.Unit = _detail.Unit;
        applied.ComponentId = SelectedComponent.ComponentId;
        if (SelectedRange is null)
        {
            RangesForSelectedComponent.Add(applied);
            SelectedRange = applied;
            return;
        }

        var index = RangesForSelectedComponent.IndexOf(SelectedRange);
        if (index >= 0)
        {
            RangesForSelectedComponent[index] = applied;
            SelectedRange = applied;
        }
    }

    private static TestComponent CloneComponent(TestComponent component)
    {
        return new TestComponent
        {
            ComponentId = component.ComponentId,
            TesttypeId = component.TesttypeId,
            ComponentCode = component.ComponentCode,
            ComponentNameEn = component.ComponentNameEn,
            ComponentNameAr = component.ComponentNameAr,
            Unit = component.Unit,
            ResultType = component.ResultType,
            DecimalPlaces = component.DecimalPlaces,
            SortOrder = component.SortOrder,
            IsActive = component.IsActive
        };
    }
}
