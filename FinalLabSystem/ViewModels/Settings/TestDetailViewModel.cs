using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestDetailViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private readonly IDialogService _dialogService;
    private TestType _editableTest = CreateEmptyTest();
    private bool _isDirty;
    private decimal _patientPrice;
    private decimal _labToLabPrice;
    private string? _validationMessage;
    private int? _selectedCollectionTypeId;
    private string? _referenceType;
    private string _tube1 = string.Empty;
    private string _tube2 = string.Empty;
    private string _tube3 = string.Empty;
    private string? _barcodeName;

    private TestType? _baselineTest;
    private int? _baselineCollectionTypeId;
    private decimal _baselinePatientPrice;
    private decimal _baselineLabToLabPrice;
    private string? _baselineTube1;
    private string? _baselineTube2;
    private string? _baselineTube3;
    private string? _baselineReferenceType;
    private string? _baselineBarcodeName;
    private bool _baselineHasPatientQuestion;
    private ICommand? _addComponentCommand;
    private ICommand? _deleteComponentCommand;
    private ICommand? _moveComponentUpCommand;
    private ICommand? _moveComponentDownCommand;
    private TestComponent? _selectedComponent;

    public TestDetailViewModel(ITestCatalogService testCatalogService, IDialogService dialogService)
    {
        _testCatalogService = testCatalogService;
        _dialogService = dialogService;
        OpenNormalRangesCommand = new RelayCommand(_ => OpenNormalRangesRequested?.Invoke(this, EventArgs.Empty), _ => EditableTest.TesttypeId > 0);
    }

    public event EventHandler? DirtyStateChanged;

    public event EventHandler? OpenNormalRangesRequested;

    public ObservableCollection<TestGroup> Groups { get; } = new();

    public ObservableCollection<CollectionType> CollectionTypes { get; } = new();

    private List<ReferenceClassification> _referenceClassifications = new();
    public List<ReferenceClassification> ReferenceClassifications
    {
        get => _referenceClassifications;
        set => SetProperty(ref _referenceClassifications, value);
    }

    public ObservableCollection<TestComponent> Components { get; } = new();

    public int? SelectedCollectionTypeId
    {
        get => _selectedCollectionTypeId;
        set
        {
            if (SetProperty(ref _selectedCollectionTypeId, value))
                MarkEntityDirty();
            OnPropertyChanged(nameof(SelectedCollectionType));
        }
    }

    public CollectionType? SelectedCollectionType
    {
        get => CollectionTypes.FirstOrDefault(ct => ct.CollectionTypeId == _selectedCollectionTypeId);
    }

    public TestComponent? SelectedComponent
    {
        get => _selectedComponent;
        set
        {
            if (SetProperty(ref _selectedComponent, value))
            {
                OnPropertyChanged(nameof(IsComponentSelected));
            }
        }
    }

    public bool IsComponentSelected => SelectedComponent is not null;

    public string? ReferenceType { get => _referenceType; set => SetProperty(ref _referenceType, value); }

    public string Tube1 { get => _tube1; set => SetProperty(ref _tube1, value); }

    public string Tube2 { get => _tube2; set => SetProperty(ref _tube2, value); }

    public string Tube3 { get => _tube3; set => SetProperty(ref _tube3, value); }

    private List<string> _availableTubeTypes = new();
    public List<string> AvailableTubeTypes
    {
        get => _availableTubeTypes;
        private set => SetProperty(ref _availableTubeTypes, value);
    }

    public string? BarcodeName { get => _barcodeName; set => SetProperty(ref _barcodeName, value); }

    public IReadOnlyList<TestRowViewModel> AllTests { get; private set; } = Array.Empty<TestRowViewModel>();

    public TestType EditableTest
    {
        get => _editableTest;
        private set => SetProperty(ref _editableTest, value);
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (SetProperty(ref _isDirty, value))
                DirtyStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string? ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public int TesttypeId => EditableTest.TesttypeId;

    public string TypeCode
    {
        get => EditableTest.TypeCode;
        set => SetTestProperty(EditableTest.TypeCode, value ?? string.Empty, v => EditableTest.TypeCode = v);
    }

    public string TypeNameEn
    {
        get => EditableTest.TypeNameEn;
        set => SetTestProperty(EditableTest.TypeNameEn, value ?? string.Empty, v => EditableTest.TypeNameEn = v);
    }

    public string? TypeNameAr
    {
        get => EditableTest.TypeNameAr;
        set => SetTestProperty(EditableTest.TypeNameAr, value, v => EditableTest.TypeNameAr = v);
    }

    public string? HistoryName
    {
        get => EditableTest.HistoryName;
        set => SetTestProperty(EditableTest.HistoryName, value, v => EditableTest.HistoryName = v);
    }

    public string? ReportNameLine1
    {
        get => EditableTest.ReportNameLine1;
        set => SetTestProperty(EditableTest.ReportNameLine1, value, v => EditableTest.ReportNameLine1 = v);
    }

    public string? ReportNameLine2
    {
        get => EditableTest.ReportNameLine2;
        set => SetTestProperty(EditableTest.ReportNameLine2, value, v => EditableTest.ReportNameLine2 = v);
    }

    public string? BillNameLine1
    {
        get => EditableTest.BillNameLine1;
        set => SetTestProperty(EditableTest.BillNameLine1, value, v => EditableTest.BillNameLine1 = v);
    }

    public string? BillNameLine2
    {
        get => EditableTest.BillNameLine2;
        set => SetTestProperty(EditableTest.BillNameLine2, value, v => EditableTest.BillNameLine2 = v);
    }

    public int GroupId
    {
        get => EditableTest.GroupId;
        set => SetTestProperty(EditableTest.GroupId, value, v => EditableTest.GroupId = v);
    }

    public short SortOrder
    {
        get => EditableTest.SortOrder;
        set => SetTestProperty(EditableTest.SortOrder, value, v => EditableTest.SortOrder = v);
    }

    public short TurnaroundHours
    {
        get => EditableTest.TurnaroundHours;
        set => SetTestProperty(EditableTest.TurnaroundHours, value, v => EditableTest.TurnaroundHours = v);
    }

    public string? CollectionNotes
    {
        get => EditableTest.CollectionNotes;
        set => SetTestProperty(EditableTest.CollectionNotes, value, v => EditableTest.CollectionNotes = v);
    }

    public string? Notes
    {
        get => EditableTest.Notes;
        set => SetTestProperty(EditableTest.Notes, value, v => EditableTest.Notes = v);
    }

    public decimal PatientPrice
    {
        get => _patientPrice;
        set
        {
            if (SetProperty(ref _patientPrice, value))
                MarkEntityDirty();
        }
    }

    public decimal LabToLabPrice
    {
        get => _labToLabPrice;
        set
        {
            if (SetProperty(ref _labToLabPrice, value))
                MarkEntityDirty();
        }
    }

    public bool IsRoutineTest
    {
        get => EditableTest.IsRoutineTest;
        set => SetTestProperty(EditableTest.IsRoutineTest, value, v => EditableTest.IsRoutineTest = v);
    }

    public bool SeeReport
    {
        get => EditableTest.SeeReport;
        set => SetTestProperty(EditableTest.SeeReport, value, v => EditableTest.SeeReport = v);
    }

    public bool PrintWithOther
    {
        get => EditableTest.PrintWithOther;
        set => SetTestProperty(EditableTest.PrintWithOther, value, v => EditableTest.PrintWithOther = v);
    }

    public bool AddWithGroup
    {
        get => EditableTest.AddWithGroup;
        set => SetTestProperty(EditableTest.AddWithGroup, value, v => EditableTest.AddWithGroup = v);
    }

    public bool IsMainTest
    {
        get => EditableTest.IsMainTest;
        set => SetTestProperty(EditableTest.IsMainTest, value, v => EditableTest.IsMainTest = v);
    }

    public bool IsSendOutside
    {
        get => EditableTest.IsSendOutside;
        set
        {
            SetTestProperty(EditableTest.IsSendOutside, value, v => EditableTest.IsSendOutside = v);
            OnPropertyChanged(nameof(IsOutsideFieldsEnabled));
        }
    }

    public bool IsOutsideFieldsEnabled => IsSendOutside;

    public string? OutsideLabName
    {
        get => EditableTest.OutsideLabName;
        set => SetTestProperty(EditableTest.OutsideLabName, value, v => EditableTest.OutsideLabName = v);
    }

    public decimal? OutsideCostPrice
    {
        get => EditableTest.OutsideCostPrice;
        set => SetTestProperty(EditableTest.OutsideCostPrice, value, v => EditableTest.OutsideCostPrice = v);
    }

    private bool _hasPatientQuestion;

    public bool HasPatientQuestion
    {
        get => _hasPatientQuestion;
        set
        {
            if (SetProperty(ref _hasPatientQuestion, value))
                OnPropertyChanged(nameof(IsPatientQuestionEnabled));
        }
    }

    public bool IsPatientQuestionEnabled => HasPatientQuestion;

    public string? PatientQuestion
    {
        get => EditableTest.PatientQuestion;
        set => SetTestProperty(EditableTest.PatientQuestion, value, v => EditableTest.PatientQuestion = v);
    }

    public ICommand OpenNormalRangesCommand { get; }

    public ICommand AddComponentCommand => _addComponentCommand ??= new RelayCommand(_ => AddComponent());
    public ICommand DeleteComponentCommand => _deleteComponentCommand ??= new RelayCommand(async _ => await DeleteComponentAsync(), _ => SelectedComponent is not null);
    public ICommand MoveComponentUpCommand => _moveComponentUpCommand ??= new RelayCommand(_ => MoveComponent(-1), _ => CanMoveUp());
    public ICommand MoveComponentDownCommand => _moveComponentDownCommand ??= new RelayCommand(_ => MoveComponent(1), _ => CanMoveDown());

    public async Task InitializeLookupsAsync()
    {
        Groups.Clear();
        foreach (var group in await _testCatalogService.GetActiveGroupsAsync())
            Groups.Add(group);

        CollectionTypes.Clear();
        foreach (var ct in await _testCatalogService.GetAllCollectionTypesAsync())
            CollectionTypes.Add(ct);

        await LoadReferenceClassificationsAsync();
        await LoadTubeMaterialsAsync();
    }

    public async Task LoadReferenceClassificationsAsync()
    {
        ReferenceClassifications = await _testCatalogService.GetReferenceClassificationsAsync();
    }

    public async Task LoadTubeMaterialsAsync()
    {
        AvailableTubeTypes = (await _testCatalogService.GetAllTubeMaterialsAsync()).Select(t => t.MaterialName).ToList();
    }

    public async Task LoadAsync(int testTypeId, IReadOnlyList<TestRowViewModel> allTests)
    {
        AllTests = allTests;
        var test = await _testCatalogService.GetTestTypeDetailsAsync(testTypeId);
        if (test is null)
            return;

        EditableTest = CloneTest(test);
        SelectedCollectionTypeId = test.CollectionTypeId;
        PatientPrice = test.TestTypePrices.FirstOrDefault(p => p.Scheme.SchemeName == "Patient Price")?.Price ?? test.DefaultPrice;
        LabToLabPrice = test.TestTypePrices.FirstOrDefault(p => p.Scheme.SchemeName == "Lab-to-Lab Price")?.Price ?? 0m;
        var tubes = test.TestTypeSampleTubes.OrderBy(t => t.SortOrder).ToList();
        Tube1 = tubes.ElementAtOrDefault(0)?.SampleType ?? string.Empty;
        Tube2 = tubes.ElementAtOrDefault(1)?.SampleType ?? string.Empty;
        Tube3 = tubes.ElementAtOrDefault(2)?.SampleType ?? string.Empty;
        ReferenceType = test.ReferenceType;
        BarcodeName = test.BarcodeName;
        HasPatientQuestion = test.PatientQuestion is not null;

        InitializeComponents(test);
        SaveBaseline();
        RaiseAllFieldsChanged();
        IsDirty = false;
    }

    public void StartNew(IReadOnlyList<TestRowViewModel> allTests)
    {
        AllTests = allTests;
        EditableTest = CreateEmptyTest();
        SelectedCollectionTypeId = null;
        PatientPrice = 0m;
        LabToLabPrice = 0m;
        Tube1 = string.Empty;
        Tube2 = string.Empty;
        Tube3 = string.Empty;
        ReferenceType = null;
        BarcodeName = null;
        Components.Clear();
        RaiseAllFieldsChanged();
        IsDirty = true;
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(TypeCode))
            return Fail("كود التحليل مطلوب.");

        if (AllTests.Any(t => t.TesttypeId != TesttypeId && string.Equals(t.TypeCode, TypeCode.Trim(), StringComparison.OrdinalIgnoreCase)))
            return Fail("كود التحليل مستخدم من قبل.");

        if (string.IsNullOrWhiteSpace(TypeNameEn))
            return Fail("اسم التحليل باللغة الإنجليزية مطلوب.");

        if (GroupId <= 0)
            return Fail("مجموعة التحليل مطلوبة.");

        if (PatientPrice < 0m || LabToLabPrice < 0m)
            return Fail("الأسعار لا يمكن أن تكون سالبة.");

        if (IsSendOutside && OutsideCostPrice is null)
            return Fail("سعر تكلفة المعمل الخارجي مطلوب عند إرسال التحليل للخارج.");

        ValidationMessage = null;
        return true;
    }

    public TestType BuildEntity()
    {
        EditableTest.TypeCode = TypeCode.Trim();
        EditableTest.TypeNameEn = TypeNameEn.Trim();
        EditableTest.TypeNameAr = NullIfWhiteSpace(TypeNameAr);
        EditableTest.HistoryName = NullIfWhiteSpace(HistoryName);
        EditableTest.ReportNameLine1 = NullIfWhiteSpace(ReportNameLine1);
        EditableTest.ReportNameLine2 = NullIfWhiteSpace(ReportNameLine2);
        EditableTest.BillNameLine1 = NullIfWhiteSpace(BillNameLine1);
        EditableTest.BillNameLine2 = NullIfWhiteSpace(BillNameLine2);
        EditableTest.CollectionTypeId = SelectedCollectionTypeId;
        EditableTest.CollectionNotes = NullIfWhiteSpace(CollectionNotes);
        EditableTest.Notes = NullIfWhiteSpace(Notes);
        EditableTest.OutsideLabName = IsSendOutside ? NullIfWhiteSpace(OutsideLabName) : null;
        EditableTest.OutsideCostPrice = IsSendOutside ? OutsideCostPrice : null;
        EditableTest.PatientQuestion = NullIfWhiteSpace(PatientQuestion);
        EditableTest.ReferenceType = ReferenceType;
        EditableTest.BarcodeName = BarcodeName;
        EditableTest.DefaultPrice = PatientPrice;
        EditableTest.IsActive = true;
        return EditableTest;
    }

    public IReadOnlyList<TestTypeSampleTube> BuildTubes()
    {
        var result = new List<TestTypeSampleTube>();
        short sortOrder = 1;

        if (!string.IsNullOrWhiteSpace(Tube1))
            result.Add(new TestTypeSampleTube { TestTypeId = EditableTest.TesttypeId, SampleType = Tube1, Quantity = 1, SortOrder = sortOrder++, IsActive = true, TubeType = Tube1 });

        if (!string.IsNullOrWhiteSpace(Tube2))
            result.Add(new TestTypeSampleTube { TestTypeId = EditableTest.TesttypeId, SampleType = Tube2, Quantity = 1, SortOrder = sortOrder++, IsActive = true, TubeType = Tube2 });

        if (!string.IsNullOrWhiteSpace(Tube3))
            result.Add(new TestTypeSampleTube { TestTypeId = EditableTest.TesttypeId, SampleType = Tube3, Quantity = 1, SortOrder = sortOrder++, IsActive = true, TubeType = Tube3 });

        return result;
    }

    public void AcceptChanges()
    {
        IsDirty = false;
        SaveBaseline();
    }

    public void SaveBaseline()
    {
        _baselineTest = CloneTest(EditableTest);
        _baselineCollectionTypeId = SelectedCollectionTypeId;
        _baselinePatientPrice = PatientPrice;
        _baselineLabToLabPrice = LabToLabPrice;
        _baselineTube1 = Tube1;
        _baselineTube2 = Tube2;
        _baselineTube3 = Tube3;
        _baselineReferenceType = ReferenceType;
        _baselineBarcodeName = BarcodeName;
        _baselineHasPatientQuestion = HasPatientQuestion;
    }

    public void CancelChanges()
    {
        if (_baselineTest is null)
            return;

        EditableTest = CloneTest(_baselineTest);
        InitializeComponents(EditableTest);
        SelectedCollectionTypeId = _baselineCollectionTypeId;
        PatientPrice = _baselinePatientPrice;
        LabToLabPrice = _baselineLabToLabPrice;
        Tube1 = _baselineTube1 ?? string.Empty;
        Tube2 = _baselineTube2 ?? string.Empty;
        Tube3 = _baselineTube3 ?? string.Empty;
        ReferenceType = _baselineReferenceType;
        BarcodeName = _baselineBarcodeName;
        HasPatientQuestion = _baselineHasPatientQuestion;
        RaiseAllFieldsChanged();
        IsDirty = false;
    }

    private bool Fail(string message)
    {
        ValidationMessage = message;
        _dialogService.ShowWarning(message, "تنبيه");
        return false;
    }

    private void SetTestProperty<T>(T oldValue, T newValue, Action<T> assign, string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

        assign(newValue);
        OnPropertyChanged(propertyName);
        MarkEntityDirty();
    }

    private void MarkEntityDirty()
    {
        IsDirty = true;
    }

    private void RaiseAllFieldsChanged()
    {
        OnPropertyChanged(nameof(TesttypeId));
        OnPropertyChanged(nameof(TypeCode));
        OnPropertyChanged(nameof(TypeNameEn));
        OnPropertyChanged(nameof(TypeNameAr));
        OnPropertyChanged(nameof(HistoryName));
        OnPropertyChanged(nameof(ReportNameLine1));
        OnPropertyChanged(nameof(ReportNameLine2));
        OnPropertyChanged(nameof(BillNameLine1));
        OnPropertyChanged(nameof(BillNameLine2));
        OnPropertyChanged(nameof(GroupId));
        OnPropertyChanged(nameof(SortOrder));
        OnPropertyChanged(nameof(TurnaroundHours));
        OnPropertyChanged(nameof(SelectedCollectionTypeId));
        OnPropertyChanged(nameof(SelectedCollectionType));
        OnPropertyChanged(nameof(CollectionNotes));
        OnPropertyChanged(nameof(Notes));
        OnPropertyChanged(nameof(PatientPrice));
        OnPropertyChanged(nameof(LabToLabPrice));
        OnPropertyChanged(nameof(IsRoutineTest));
        OnPropertyChanged(nameof(SeeReport));
        OnPropertyChanged(nameof(PrintWithOther));
        OnPropertyChanged(nameof(AddWithGroup));
        OnPropertyChanged(nameof(IsMainTest));
        OnPropertyChanged(nameof(IsSendOutside));
        OnPropertyChanged(nameof(IsOutsideFieldsEnabled));
        OnPropertyChanged(nameof(OutsideLabName));
        OnPropertyChanged(nameof(OutsideCostPrice));
        OnPropertyChanged(nameof(PatientQuestion));
        OnPropertyChanged(nameof(Tube1));
        OnPropertyChanged(nameof(Tube2));
        OnPropertyChanged(nameof(Tube3));
        OnPropertyChanged(nameof(ReferenceType));
        OnPropertyChanged(nameof(BarcodeName));
    }

    private static TestType CreateEmptyTest()
    {
        return new TestType
        {
            TypeCode = string.Empty,
            TypeNameEn = string.Empty,
            TurnaroundHours = 24,
            PrintWithOther = true,
            AddWithGroup = true,
            IsActive = true
        };
    }

    private static TestType CloneTest(TestType test)
    {
        return new TestType
        {
            TesttypeId = test.TesttypeId,
            GroupId = test.GroupId,
            TypeCode = test.TypeCode,
            TypeNameEn = test.TypeNameEn,
            TypeNameAr = test.TypeNameAr,
            TypeAbbrev = test.TypeAbbrev,
            DefaultPrice = test.DefaultPrice,
            SampleType = test.SampleType,
            DefaultTubeType = test.DefaultTubeType,
            DefaultTubeColor = test.DefaultTubeColor,
            TurnaroundHours = test.TurnaroundHours,
            IsOutsourceable = test.IsOutsourceable,
            SpecialType = test.SpecialType,
            SortOrder = test.SortOrder,
            IsActive = test.IsActive,
            Notes = test.Notes,
            ReportNameLine1 = test.ReportNameLine1,
            ReportNameLine2 = test.ReportNameLine2,
            BillNameLine1 = test.BillNameLine1,
            BillNameLine2 = test.BillNameLine2,
            HistoryName = test.HistoryName,
            CollectionTypeId = test.CollectionTypeId,
            CollectionNotes = test.CollectionNotes,
            IsRoutineTest = test.IsRoutineTest,
            SeeReport = test.SeeReport,
            PrintWithOther = test.PrintWithOther,
            AddWithGroup = test.AddWithGroup,
            IsMainTest = test.IsMainTest,
            IsSendOutside = test.IsSendOutside,
            OutsideLabName = test.OutsideLabName,
            OutsideCostPrice = test.OutsideCostPrice,
            PatientQuestion = test.PatientQuestion,
            TestComponents = test.TestComponents?.Select(tc => new TestComponent
            {
                ComponentId = tc.ComponentId,
                TesttypeId = tc.TesttypeId,
                ComponentCode = tc.ComponentCode,
                ComponentNameEn = tc.ComponentNameEn,
                ComponentNameAr = tc.ComponentNameAr,
                ResultType = tc.ResultType,
                DecimalPlaces = tc.DecimalPlaces,
                Unit = tc.Unit,
                SortOrder = tc.SortOrder,
                IsActive = tc.IsActive
            }).ToList() ?? new List<TestComponent>()
        };
    }

    private void AddComponent()
    {
        var maxSort = Components.Count > 0 ? Components.Max(c => c.SortOrder) : 0;
        var component = new TestComponent
        {
            ComponentCode = "",
            ComponentNameEn = "",
            ResultType = "NUMERIC",
            SortOrder = (short)(maxSort + 1),
            IsActive = true,
            DecimalPlaces = 2,
            TesttypeId = EditableTest.TesttypeId
        };
        Components.Add(component);
        SelectedComponent = component;
        MarkEntityDirty();
    }

    private async Task DeleteComponentAsync()
    {
        if (SelectedComponent is null) return;
        if (SelectedComponent.ComponentId > 0)
        {
            if (!_dialogService.ShowConfirmation("Are you sure you want to delete this component?", "Confirm"))
                return;
            try
            {
                await _testCatalogService.DeleteComponentAsync(SelectedComponent.ComponentId);
            }
            catch
            {
                _dialogService.ShowWarning("Cannot delete component. It may have associated ranges or results.");
                return;
            }
        }
        Components.Remove(SelectedComponent);
        SelectedComponent = null;
        MarkEntityDirty();
    }

    private bool CanMoveUp() => SelectedComponent is not null && Components.IndexOf(SelectedComponent) > 0;
    private bool CanMoveDown() => SelectedComponent is not null && Components.IndexOf(SelectedComponent) < Components.Count - 1;

    private void MoveComponent(int direction)
    {
        if (SelectedComponent is null) return;
        var idx = Components.IndexOf(SelectedComponent);
        var swapIdx = idx + direction;
        if (swapIdx < 0 || swapIdx >= Components.Count) return;

        Components.Move(idx, swapIdx);
        for (int i = 0; i < Components.Count; i++)
            Components[i].SortOrder = (short)(i + 1);
        MarkEntityDirty();
    }

    private void InitializeComponents(TestType? test)
    {
        Components.Clear();
        if (test?.TestComponents is not null)
        {
            foreach (var c in test.TestComponents.OrderBy(c => c.SortOrder))
                Components.Add(c);
        }
    }

    public IReadOnlyList<TestComponent> BuildComponents()
    {
        foreach (var c in Components)
        {
            c.TesttypeId = EditableTest.TesttypeId;
            if (c.ComponentId == 0 && string.IsNullOrWhiteSpace(c.ComponentCode))
                c.ComponentCode = EditableTest.TypeCode;
            if (c.ComponentId == 0 && string.IsNullOrWhiteSpace(c.ComponentNameEn))
                c.ComponentNameEn = EditableTest.TypeNameEn;
        }
        return Components.ToList();
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
