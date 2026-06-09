using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestDetailViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private TestType _editableTest = CreateEmptyTest();
    private bool _isDirty;
    private double _patientPrice;
    private double _labToLabPrice;
    private string? _validationMessage;
    private TestTypeSampleTubeRowViewModel? _selectedTube;
    private int? _selectedCollectionTypeId;

    public TestDetailViewModel(ITestCatalogService testCatalogService)
    {
        _testCatalogService = testCatalogService;
        AddTubeCommand = new RelayCommand(_ => AddTube());
        EditTubeCommand = new RelayCommand(_ => MarkDirty(), _ => SelectedTube is not null);
        DeleteTubeCommand = new RelayCommand(_ => DeleteTube(), _ => SelectedTube is not null);
        OpenNormalRangesCommand = new RelayCommand(_ => OpenNormalRangesRequested?.Invoke(this, EventArgs.Empty), _ => EditableTest.TesttypeId > 0);
        Tubes.CollectionChanged += OnTubesCollectionChanged;
    }

    public event EventHandler? DirtyStateChanged;

    public event EventHandler? OpenNormalRangesRequested;

    public ObservableCollection<TestGroup> Groups { get; } = new();

    public ObservableCollection<CollectionType> CollectionTypes { get; } = new();

    public int? SelectedCollectionTypeId
    {
        get => _selectedCollectionTypeId;
        set => SetProperty(ref _selectedCollectionTypeId, value);
    }

    public CollectionType? SelectedCollectionType
    {
        get => CollectionTypes.FirstOrDefault(ct => ct.CollectionTypeId == _selectedCollectionTypeId);
    }

    public ObservableCollection<TestTypeSampleTubeRowViewModel> Tubes { get; } = new();

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

    public TestTypeSampleTubeRowViewModel? SelectedTube
    {
        get => _selectedTube;
        set => SetProperty(ref _selectedTube, value);
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

    public double PatientPrice
    {
        get => _patientPrice;
        set
        {
            if (SetProperty(ref _patientPrice, value))
                MarkDirty();
        }
    }

    public double LabToLabPrice
    {
        get => _labToLabPrice;
        set
        {
            if (SetProperty(ref _labToLabPrice, value))
                MarkDirty();
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

    public string? PatientQuestion
    {
        get => EditableTest.PatientQuestion;
        set => SetTestProperty(EditableTest.PatientQuestion, value, v => EditableTest.PatientQuestion = v);
    }

    public ICommand AddTubeCommand { get; }

    public ICommand EditTubeCommand { get; }

    public ICommand DeleteTubeCommand { get; }

    public ICommand OpenNormalRangesCommand { get; }

    public async Task InitializeLookupsAsync()
    {
        Groups.Clear();
        foreach (var group in await _testCatalogService.GetActiveGroupsAsync())
            Groups.Add(group);

        CollectionTypes.Clear();
        foreach (var ct in await _testCatalogService.GetAllCollectionTypesAsync())
            CollectionTypes.Add(ct);
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
        LabToLabPrice = test.TestTypePrices.FirstOrDefault(p => p.Scheme.SchemeName == "Lab-to-Lab Price")?.Price ?? 0d;
        Tubes.Clear();
        foreach (var tube in test.TestTypeSampleTubes.OrderBy(t => t.SortOrder))
            Tubes.Add(new TestTypeSampleTubeRowViewModel(tube));

        RaiseAllFieldsChanged();
        IsDirty = false;
    }

    public void StartNew(IReadOnlyList<TestRowViewModel> allTests)
    {
        AllTests = allTests;
        EditableTest = CreateEmptyTest();
        SelectedCollectionTypeId = null;
        PatientPrice = 0d;
        LabToLabPrice = 0d;
        Tubes.Clear();
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

        if (PatientPrice < 0 || LabToLabPrice < 0)
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
        EditableTest.DefaultPrice = PatientPrice;
        EditableTest.IsActive = true;
        return EditableTest;
    }

    public IReadOnlyList<TestTypeSampleTube> BuildTubes()
        => Tubes.Select(t => t.ToEntity()).ToList();

    public void AcceptChanges()
    {
        IsDirty = false;
    }

    private void AddTube()
    {
        var nextSort = Tubes.Count == 0 ? (short)1 : (short)(Tubes.Max(t => t.SortOrder) + 1);
        var row = new TestTypeSampleTubeRowViewModel
        {
            TestTypeId = EditableTest.TesttypeId,
            TubeType = "Default",
            Quantity = 1,
            SortOrder = nextSort,
            IsActive = true
        };
        Tubes.Add(row);
        SelectedTube = row;
        MarkDirty();
    }

    private void OnTubesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (TestTypeSampleTubeRowViewModel row in e.OldItems)
                row.PropertyChanged -= OnTubePropertyChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (TestTypeSampleTubeRowViewModel row in e.NewItems)
                row.PropertyChanged += OnTubePropertyChanged;
        }
    }

    private void OnTubePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        MarkDirty();
    }

    private void DeleteTube()
    {
        if (SelectedTube is null)
            return;

        Tubes.Remove(SelectedTube);
        SelectedTube = null;
        MarkDirty();
    }

    private bool Fail(string message)
    {
        ValidationMessage = message;
        MessageBox.Show(message, "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    private void SetTestProperty<T>(T oldValue, T newValue, Action<T> assign, string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

        assign(newValue);
        OnPropertyChanged(propertyName);
        MarkDirty();
    }

    private void MarkDirty()
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
            PatientQuestion = test.PatientQuestion
        };
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
