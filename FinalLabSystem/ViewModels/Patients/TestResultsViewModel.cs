using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class TestResultsViewModel : ViewModelBase
{
    private readonly IVisitService _visitService;
    private readonly IRoutineResultService _routineResultService;
    private readonly IAuditService _auditService;
    private readonly IReportingService _reportingService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly INavigationService _navigationService;
    private readonly IPrintService _printService;

    private ObservableCollection<TodayPatientWithStatusDto> _allPatients = new();
    private TodayPatientWithStatusDto? _selectedPatient;
    private string _searchText = string.Empty;
    private PatientVisitStatus? _filterMode;
    private VisitFullDto? _currentPatientInfo;
    private ObservableCollection<VisitTestItemDto> _patientTests = new();
    private VisitTestItemDto? _selectedTest;
    private bool _isBusy;
    private bool _canAccessAuditFeatures;
    private string _patientNotes = string.Empty;
    private DateTime _selectedDate = DateTime.Today;
    private bool _isInlineEditing;
    private VisitTestItemDto? _editingTest;
    private string _inlineResultValue = string.Empty;
    private string _quickNoteText = string.Empty;
    private string _selectedPatientType = "All";

    public TestResultsViewModel(
        IVisitService visitService,
        IRoutineResultService routineResultService,
        IAuditService auditService,
        IReportingService reportingService,
        IAuthService authService,
        IDialogService dialogService,
        ICurrentUserSession currentUserSession,
        INavigationService navigationService,
        IPrintService printService)
    {
        _visitService = visitService;
        _routineResultService = routineResultService;
        _auditService = auditService;
        _reportingService = reportingService;
        _authService = authService;
        _dialogService = dialogService;
        _currentUserSession = currentUserSession;
        _navigationService = navigationService;
        _printService = printService;

        PatientsView = CollectionViewSource.GetDefaultView(AllPatients);
        PatientsView.Filter = FilterPatient;

        TestsView = CollectionViewSource.GetDefaultView(PatientTests);
        TestsView.Filter = FilterTest;

        ReturnToMainCommand = new RelayCommand(_ => navigationService.ReturnToMain());
        RefreshCommand = new AsyncRelayCommand(async _ => await LoadAsync());
        SelectPatientCommand = new AsyncRelayCommand<object>(async param => await SelectPatientAsync(param));
        ApplyFilterCommand = new RelayCommand(ApplyFilter);
        ClearFilterCommand = new RelayCommand(_ => { FilterMode = null; PatientsView.Refresh(); });
        NavigateDayCommand = new AsyncRelayCommand(async param => await NavigateDayAsync(param));
        EnterResultCommand = new AsyncRelayCommand(async _ => await EnterResultAsync(), _ => SelectedTest != null && HasSelectedPatient);
        ManualOverrideCommand = new AsyncRelayCommand(async _ => await ManualOverrideAsync(), _ => SelectedTest != null && HasSelectedPatient);
        UndoManualOverrideCommand = new AsyncRelayCommand(async _ => await UndoManualOverrideAsync(), _ => SelectedTest != null && SelectedTest.IsManuallyOverridden && HasSelectedPatient);
        AddEditNotesCommand = new RelayCommand(_ => AddEditNotes(), _ => HasSelectedPatient);
        ShowAuditPCommand = new AsyncRelayCommand(async _ => await ShowAuditPAsync(), _ => HasSelectedPatient && CanAccessAuditFeatures);
        ShowAuditTCommand = new AsyncRelayCommand(async _ => await ShowAuditTAsync(), _ => SelectedTest != null && CanAccessAuditFeatures);
        CopyCodeCommand = new RelayCommand(_ => CopyCode(), _ => CurrentPatientInfo != null);
        OpenPatientDataCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<PatientRegistrationViewModel>());
        PrintCompositeReportCommand = new AsyncRelayCommand(async _ => await PrintCompositeReportAsync(), _ => HasSelectedPatient);
        PrintWorksheetCommand = new AsyncRelayCommand(async _ => await PrintWorksheetAsync(), _ => HasSelectedPatient);
        PrintEnvelopeCommand = new AsyncRelayCommand(async _ => await PrintEnvelopeAsync(), _ => HasSelectedPatient);
        PrintMedicalHistoryCommand = new AsyncRelayCommand(async _ => await PrintMedicalHistoryAsync(), _ => HasSelectedPatient && SelectedTest != null);
        PrintBlankReportCommand = new AsyncRelayCommand(async _ => await PrintBlankReportAsync(), _ => HasSelectedPatient);

        SaveInlineResultCommand = new AsyncRelayCommand(async _ => await SaveSelectedTestResultAsync(), _ => SelectedTest != null && HasSelectedPatient);
        CancelInlineEditCommand = new RelayCommand(_ => CancelInlineEdit(), _ => IsInlineEditing);
        HandleRowActivateCommand = new AsyncRelayCommand<object>(async param => await HandleRowActivateAsync(param));
        TogglePrintCommand = new AsyncRelayCommand(async _ => await TogglePrintAsync(), _ => SelectedTest != null && HasSelectedPatient);
        ToggleExportCommand = new AsyncRelayCommand(async _ => await ToggleExportAsync(), _ => SelectedTest != null && HasSelectedPatient);
        MarkReviewedCommand = new AsyncRelayCommand(async _ => await MarkReviewedAsync(), _ => SelectedTest != null && HasSelectedPatient);
        SaveQuickNoteCommand = new AsyncRelayCommand(async _ => await SaveQuickNoteAsync(), _ => HasSelectedPatient);
        SetPatientTypeCommand = new RelayCommand(param => SelectedPatientType = param as string ?? "All");
        OpenSearchCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<PatientSearchViewModel>());
        OpenDeliveryCommand = new RelayCommand(_ => navigationService.OpenTaskWindow<DeliveryViewModel>());
        ToggleReviewStatusCommand = new AsyncRelayCommand(async _ => await ToggleReviewStatusAsync(), _ => SelectedTest != null && HasSelectedPatient);
        ToggleFinishStatusCommand = new AsyncRelayCommand(async _ => await ToggleFinishStatusAsync(), _ => SelectedTest != null && HasSelectedPatient);
        TogglePrintStatusCommand = new AsyncRelayCommand(async _ => await TogglePrintStatusAsync(), _ => SelectedTest != null && HasSelectedPatient);
        PreviewReportCommand = new AsyncRelayCommand(async _ => await PreviewReportAsync(), _ => HasSelectedPatient);
        SendSmsCommand = new AsyncRelayCommand(async _ => await SendSmsAsync(), _ => HasSelectedPatient);
    }

    public ObservableCollection<TodayPatientWithStatusDto> AllPatients
    {
        get => _allPatients;
        set => SetProperty(ref _allPatients, value);
    }

    public ICollectionView PatientsView { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                PatientsView.Refresh();
        }
    }

    public TodayPatientWithStatusDto? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            if (SetProperty(ref _selectedPatient, value))
            {
                OnPropertyChanged(nameof(HasSelectedPatient));
                _ = SelectPatientAsync(value);
            }
        }
    }

    public PatientVisitStatus? FilterMode
    {
        get => _filterMode;
        set
        {
            if (SetProperty(ref _filterMode, value))
                PatientsView.Refresh();
        }
    }

    public VisitFullDto? CurrentPatientInfo
    {
        get => _currentPatientInfo;
        set
        {
            if (SetProperty(ref _currentPatientInfo, value))
                PatientNotes = value?.Notes ?? string.Empty;
        }
    }

    public ObservableCollection<VisitTestItemDto> PatientTests
    {
        get => _patientTests;
        set => SetProperty(ref _patientTests, value);
    }

    public ICollectionView TestsView { get; }

    public VisitTestItemDto? SelectedTest
    {
        get => _selectedTest;
        set => SetProperty(ref _selectedTest, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public bool HasSelectedPatient => SelectedPatient != null;

    public bool CanAccessAuditFeatures
    {
        get => _canAccessAuditFeatures;
        set => SetProperty(ref _canAccessAuditFeatures, value);
    }

    public string PatientNotes
    {
        get => _patientNotes;
        set => SetProperty(ref _patientNotes, value);
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
                OnPropertyChanged(nameof(SelectedDateDisplay));
        }
    }

    public string SelectedDateDisplay => SelectedDate.ToString("yyyy-MM-dd");

    public bool IsInlineEditing
    {
        get => _isInlineEditing;
        set => SetProperty(ref _isInlineEditing, value);
    }

    public VisitTestItemDto? EditingTest
    {
        get => _editingTest;
        set => SetProperty(ref _editingTest, value);
    }

    public string InlineResultValue
    {
        get => _inlineResultValue;
        set => SetProperty(ref _inlineResultValue, value);
    }

    public string QuickNoteText
    {
        get => _quickNoteText;
        set => SetProperty(ref _quickNoteText, value);
    }

    public string SelectedPatientType
    {
        get => _selectedPatientType;
        set
        {
            if (SetProperty(ref _selectedPatientType, value))
            {
                PatientsView.Refresh();
                OnPropertyChanged(nameof(PatientCount));
            }
        }
    }

    public int PatientCount => PatientsView.Cast<object>().Count();

    public bool CanPrint => HasSelectedPatient &&
        PatientTests.Count > 0 &&
        PatientTests.All(t => t.ComponentResults.All(
            c => c.ValidationStatus >= ResultValidationStatus.Reviewed));

    public ICommand ReturnToMainCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SelectPatientCommand { get; }
    public ICommand ApplyFilterCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand NavigateDayCommand { get; }
    public ICommand EnterResultCommand { get; }
    public ICommand ManualOverrideCommand { get; }
    public ICommand UndoManualOverrideCommand { get; }
    public ICommand AddEditNotesCommand { get; }
    public ICommand ShowAuditPCommand { get; }
    public ICommand ShowAuditTCommand { get; }
    public ICommand CopyCodeCommand { get; }
    public ICommand OpenPatientDataCommand { get; }
    public ICommand PrintCompositeReportCommand { get; }
    public ICommand PrintWorksheetCommand { get; }
    public ICommand PrintEnvelopeCommand { get; }
    public ICommand PrintMedicalHistoryCommand { get; }
    public ICommand PrintBlankReportCommand { get; }
    public ICommand SaveInlineResultCommand { get; }
    public ICommand CancelInlineEditCommand { get; }
    public ICommand HandleRowActivateCommand { get; }
    public ICommand TogglePrintCommand { get; }
    public ICommand ToggleExportCommand { get; }
    public ICommand MarkReviewedCommand { get; }
    public ICommand SaveQuickNoteCommand { get; }
    public ICommand SetPatientTypeCommand { get; }
    public ICommand OpenSearchCommand { get; }
    public ICommand OpenDeliveryCommand { get; }
    public ICommand ToggleReviewStatusCommand { get; }
    public ICommand ToggleFinishStatusCommand { get; }
    public ICommand TogglePrintStatusCommand { get; }
    public ICommand PreviewReportCommand { get; }
    public ICommand SendSmsCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            AllPatients.Clear();
            var patients = await _visitService.GetTodayPatientsWithStatusAsync(SelectedDate);
            foreach (var patient in patients)
                AllPatients.Add(patient);

            if (_currentUserSession.CurrentUser != null)
                CanAccessAuditFeatures = await _authService.HasPermissionAsync(
                    _currentUserSession.CurrentUser.StaffId, "RESULTS.VIEW_AUDIT");

            PatientsView.Refresh();
            OnPropertyChanged(nameof(PatientCount));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SelectPatientAsync(object? param)
    {
        var patient = param as TodayPatientWithStatusDto ?? SelectedPatient;
        if (patient == null) return;

        IsBusy = true;
        try
        {
            CurrentPatientInfo = await _visitService.GetVisitFullDataAsync(patient.VisitId);

            var visit = await _visitService.GetVisitSummaryAsync(patient.VisitId);
            PatientTests.Clear();

            if (visit?.VisitTests != null)
            {
                foreach (var vt in visit.VisitTests)
                {
                    var results = await _routineResultService.GetResultsByVisitTestAsync(vt.VisitTestId);

                    var componentResults = new ObservableCollection<TestComponentResultDto>();
                    if (vt.Testtype?.TestComponents != null)
                    {
                        foreach (var comp in vt.Testtype.TestComponents.OrderBy(c => c.SortOrder))
                        {
                            var result = results.FirstOrDefault(r => r.ComponentId == comp.ComponentId);
                            componentResults.Add(new TestComponentResultDto
                            {
                                ComponentId = comp.ComponentId,
                                ComponentCode = comp.ComponentCode,
                                ComponentName = comp.ComponentNameAr ?? comp.ComponentNameEn,
                                Unit = comp.Unit,
                                ResultType = comp.ResultType,
                                DecimalPlaces = comp.DecimalPlaces,
                                SortOrder = comp.SortOrder,
                                ResultId = result?.ResultId,
                                ResultValue = result?.ResultValue,
                                ResultNumeric = result?.ResultNumeric,
                                ResultStatus = result?.ResultStatus,
                                Comment = result?.Comment,
                                SnapUnit = result?.SnapUnit,
                                SnapLowNormal = result?.SnapLowNormal,
                                SnapHighNormal = result?.SnapHighNormal,
                                SnapNormalText = result?.SnapNormalText,
                                ValidationStatus = result?.ValidationStatus ?? ResultValidationStatus.Entered,
                                EnteredByName = result?.EnteredByNavigation?.DisplayName,
                                EnteredAt = result?.EnteredAt,
                                LastModifiedByName = result?.LastModifiedByNavigation?.DisplayName,
                                LastModifiedAt = result?.LastModifiedAt
                            });
                        }
                    }

                    var isManuallyOverridden = vt.TestWorkflows?.Any(w => w.Stage == "MANUAL_COMPLETE") == true;

                    PatientTests.Add(new VisitTestItemDto
                    {
                        VisitTestId = vt.VisitTestId,
                        TestTypeName = vt.Testtype?.TypeNameAr ?? vt.Testtype?.TypeNameEn ?? "Unknown",
                        TestTypeCode = vt.Testtype?.TypeCode ?? string.Empty,
                        SpecialType = vt.Testtype?.SpecialType,
                        CurrentStage = vt.CurrentStage,
                        IsOutsourced = vt.IsOutsourced,
                        ExternalLabName = null,
                        TotalComponents = componentResults.Count,
                        EnteredCount = componentResults.Count(c => c.ResultValue != null),
                        ReviewedCount = componentResults.Count(c => c.ValidationStatus >= ResultValidationStatus.Reviewed),
                        OverallValidationStatus = componentResults.Any()
                            ? componentResults.Min(c => c.ValidationStatus)
                            : ResultValidationStatus.Entered,
                        IsManuallyOverridden = isManuallyOverridden,
                        IsPrinted = vt.IsPrinted,
                        IsExported = vt.IsExported,
                        ComponentResults = componentResults
                    });
                }
            }

            TestsView.Refresh();
            OnPropertyChanged(nameof(CanPrint));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool FilterPatient(object item)
    {
        if (item is not TodayPatientWithStatusDto patient)
            return false;

        var term = SearchText?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            if (!patient.PatientCode.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                !patient.FullNameAr.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                !(patient.VisitCode?.Contains(term, StringComparison.OrdinalIgnoreCase) == true) &&
                !(int.TryParse(term, out var attNum) && patient.AttendanceNumber == attNum))
                return false;
        }

        if (!string.Equals(SelectedPatientType, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(patient.PatientType, SelectedPatientType, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        if (FilterMode.HasValue)
            return patient.ComputedStatus == FilterMode.Value;

        return true;
    }

    private bool FilterTest(object item)
    {
        return item is VisitTestItemDto;
    }

    private void ApplyFilter(object? parameter)
    {
        if (parameter is string statusStr && Enum.TryParse<PatientVisitStatus>(statusStr, out var status))
            FilterMode = status;
        else
            FilterMode = null;
    }

    private async Task NavigateDayAsync(object? parameter)
    {
        if (parameter is int days)
        {
            SelectedDate = SelectedDate.AddDays(days);
            await LoadAsync();
        }
    }

    private async Task EnterResultAsync()
    {
        if (SelectedTest == null) return;

        if (SelectedTest.TotalComponents == 1)
        {
            EditingTest = SelectedTest;
            InlineResultValue = SelectedTest.ComponentResults.Count == 1
                ? SelectedTest.ComponentResults[0].ResultValue ?? string.Empty
                : string.Empty;
            IsInlineEditing = true;
        }
        else
        {
            await OpenMultiComponentEditorAsync(SelectedTest);
        }
    }

    private async Task HandleRowActivateAsync(object? parameter)
    {
        await EnterResultAsync();
    }

    private async Task SaveInlineResultAsync()
    {
        if (EditingTest == null || !HasSelectedPatient) return;

        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        var patientId = CurrentPatientInfo?.PatientId ?? 0;
        var component = EditingTest.ComponentResults.FirstOrDefault();
        if (component == null) return;

        await _routineResultService.SaveSingleComponentResultAsync(
            EditingTest.VisitTestId, component.ComponentId,
            InlineResultValue, patientId, staffId);

        IsInlineEditing = false;
        EditingTest = null;

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task SaveSelectedTestResultAsync()
    {
        if (SelectedTest == null || !HasSelectedPatient) return;

        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        var patientId = CurrentPatientInfo?.PatientId ?? 0;
        var component = SelectedTest.ComponentResults.FirstOrDefault();
        if (component == null) return;

        await _routineResultService.SaveSingleComponentResultAsync(
            SelectedTest.VisitTestId, component.ComponentId,
            SelectedTest.SingleComponentResultValue, patientId, staffId);

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private void CancelInlineEdit()
    {
        IsInlineEditing = false;
        EditingTest = null;
        InlineResultValue = string.Empty;
    }

    private async Task TogglePrintAsync()
    {
        if (SelectedTest == null) return;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        await _routineResultService.TogglePrintStatusAsync(SelectedTest.VisitTestId, staffId);

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task ToggleExportAsync()
    {
        if (SelectedTest == null) return;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        if (!SelectedTest.IsPrinted)
        {
            _dialogService.ShowWarning(
                "يجب طباعة النتائج أولاً قبل التصدير.\nيرجى الضغط على زر 'طبعت' أولاً.", "تنبيه");
            return;
        }

        await _routineResultService.ToggleExportStatusAsync(SelectedTest.VisitTestId, staffId);

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task ToggleReviewStatusAsync()
    {
        if (SelectedTest == null) return;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        var action = SelectedTest.IsAllReviewed ? "UNREVIEWED" : "REVIEWED";
        await _auditService.LogActionAsync(
            "VisitTest", SelectedTest.VisitTestId, action, staffId,
            $"{(SelectedTest.IsAllReviewed ? "إلغاء مراجعة" : "مراجعة")} بواسطة {_currentUserSession.CurrentUser?.DisplayName}");

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task ToggleFinishStatusAsync()
    {
        if (SelectedTest == null) return;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        if (SelectedTest.IsManuallyOverridden)
        {
            await _auditService.LogActionAsync(
                "VisitTest", SelectedTest.VisitTestId, "MANUAL_UNDO", staffId,
                $"إلغاء التجاوز بواسطة {_currentUserSession.CurrentUser?.DisplayName}");
        }
        else
        {
            await _auditService.LogActionAsync(
                "VisitTest", SelectedTest.VisitTestId, "MANUAL_COMPLETE", staffId,
                $"تجاوز يدوي بواسطة {_currentUserSession.CurrentUser?.DisplayName}");

            await _visitService.UpdateVisitTestsAsync(
                SelectedPatient!.VisitId,
                PatientTests.Select(t => t.VisitTestId).ToList());
        }

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task TogglePrintStatusAsync()
    {
        if (SelectedTest == null) return;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        await _routineResultService.TogglePrintStatusAsync(SelectedTest.VisitTestId, staffId);

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task MarkReviewedAsync()
    {
        if (SelectedTest == null) return;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        var confirm = MessageBox.Show(
            $"هل تريد مراجعة التحليل \"{SelectedTest.TestTypeName}\"؟",
            "تأكيد المراجعة", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        await _auditService.LogActionAsync(
            "VisitTest", SelectedTest.VisitTestId, "REVIEWED", staffId,
            $"مراجعة بواسطة {_currentUserSession.CurrentUser?.DisplayName}");

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task OpenMultiComponentEditorAsync(VisitTestItemDto test)
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        var patientId = CurrentPatientInfo?.PatientId ?? 0;

        var vm = new ResultEntryViewModel(
            _routineResultService,
            _visitService,
            _auditService,
            _currentUserSession,
            test.VisitTestId,
            patientId,
            test.TestTypeName,
            new ObservableCollection<TestComponentResultDto>(test.ComponentResults));

        var window = new Views.Patients.ResultEntryWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() == true)
        {
            if (SelectedPatient != null)
                await SelectPatientAsync(SelectedPatient);
        }
    }

    private async Task ManualOverrideAsync()
    {
        if (SelectedTest == null) return;

        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        var confirm = MessageBox.Show(
            $"هل تريد تجاوز التحليل \"{SelectedTest.TestTypeName}\" يدوياً؟",
            "تأكيد التجاوز", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        await _auditService.LogActionAsync(
            "VisitTest", SelectedTest.VisitTestId, "MANUAL_COMPLETE", staffId,
            $"تجاوز يدوي بواسطة {_currentUserSession.CurrentUser?.DisplayName}");

        await _visitService.UpdateVisitTestsAsync(
            SelectedPatient!.VisitId,
            PatientTests.Select(t => t.VisitTestId).ToList());

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private async Task UndoManualOverrideAsync()
    {
        if (SelectedTest == null || !SelectedTest.IsManuallyOverridden) return;

        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        if (staffId == 0) return;

        var confirm = MessageBox.Show(
            $"هل تريد التراجع عن التجاوز اليدوي للتحليل \"{SelectedTest.TestTypeName}\"؟",
            "تأكيد التراجع", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        await _auditService.LogActionAsync(
            "VisitTest", SelectedTest.VisitTestId, "MANUAL_UNDO", staffId,
            $"إلغاء التجاوز بواسطة {_currentUserSession.CurrentUser?.DisplayName}");

        if (SelectedPatient != null)
            await SelectPatientAsync(SelectedPatient);
    }

    private void AddEditNotes()
    {
        if (CurrentPatientInfo == null) return;

        var input = ShowInputDialog("ملاحظات المريض", CurrentPatientInfo.VisitNotes ?? string.Empty);
        if (input != null)
        {
            CurrentPatientInfo.VisitNotes = input;
            PatientNotes = input;
        }
    }

    private static string? ShowInputDialog(string title, string defaultValue)
    {
        var window = new Window
        {
            Title = title,
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            FlowDirection = FlowDirection.RightToLeft,
            ResizeMode = ResizeMode.NoResize,
            Owner = Application.Current.MainWindow
        };

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = defaultValue,
            Height = 60,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            Margin = new Thickness(8)
        };

        var okButton = new System.Windows.Controls.Button
        {
            Content = "حفظ",
            Width = 80,
            Height = 30,
            Margin = new Thickness(4),
            IsDefault = true
        };
        okButton.Click += (s, e) => { window.DialogResult = true; window.Close(); };

        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "إلغاء",
            Width = 80,
            Height = 30,
            Margin = new Thickness(4),
            IsCancel = true
        };

        var stack = new System.Windows.Controls.StackPanel();
        stack.Children.Add(textBox);

        var btnPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8, 0, 8, 8)
        };
        btnPanel.Children.Add(okButton);
        btnPanel.Children.Add(cancelButton);
        stack.Children.Add(btnPanel);

        window.Content = stack;

        return window.ShowDialog() == true ? textBox.Text : null;
    }

    private void CopyCode()
    {
        if (CurrentPatientInfo != null)
            Clipboard.SetText(CurrentPatientInfo.PatientCode);
    }

    private async Task SaveQuickNoteAsync()
    {
        if (CurrentPatientInfo == null) return;

        await _visitService.UpdateVisitNotesAsync(CurrentPatientInfo.VisitId, QuickNoteText);
        CurrentPatientInfo.VisitNotes = QuickNoteText;
    }

    private async Task ShowAuditPAsync()
    {
        if (CurrentPatientInfo == null) return;

        var logs = await _auditService.GetTableAuditHistoryAsync(
            "Visit", CurrentPatientInfo.VisitId);

        var vm = new AuditTrailViewModel("سجل تدقيق الزيارة", logs);
        var window = new Views.Patients.AuditTrailWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    private async Task ShowAuditTAsync()
    {
        if (SelectedTest == null) return;

        var logs = await _auditService.GetResultModificationsAsync(SelectedTest.VisitTestId);

        var vm = new AuditTrailViewModel("سجل تدقيق التحليل", logs);
        var window = new Views.Patients.AuditTrailWindow
        {
            DataContext = vm,
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    private async Task PrintCompositeReportAsync()
    {
        if (CurrentPatientInfo == null) return;

        var allReviewed = PatientTests.All(t =>
            t.ComponentResults.All(c =>
                c.ValidationStatus >= ResultValidationStatus.Reviewed));

        if (!allReviewed)
        {
            _dialogService.ShowWarning(
                "يجب مراجعة جميع النتائج قبل الطباعة.", "تنبيه");
            return;
        }

        await _printService.PrintAsync("CompositeReport", CurrentPatientInfo);
    }

    private async Task PrintWorksheetAsync()
    {
        if (CurrentPatientInfo == null) return;

        var pending = await _reportingService.GetPendingWorksheetsAsync();
        var forVisit = pending.Where(p => p.VisitId == CurrentPatientInfo.VisitId).ToList();
        await _printService.PrintAsync("Worksheet", forVisit);
    }

    private async Task PrintEnvelopeAsync()
    {
        if (CurrentPatientInfo == null) return;
        await _printService.PrintAsync("Envelope", CurrentPatientInfo);
    }

    private async Task PrintMedicalHistoryAsync()
    {
        if (CurrentPatientInfo == null || SelectedTest == null) return;

        var history = await _reportingService.GetHistoricalComparisonsAsync(
            CurrentPatientInfo.PatientId,
            PatientTests.FirstOrDefault(t => t.VisitTestId == SelectedTest.VisitTestId)?.VisitTestId ?? 0);
        await _printService.PrintAsync("MedicalHistory", history);
    }

    private async Task PrintBlankReportAsync()
    {
        if (CurrentPatientInfo == null) return;
        await _printService.PrintAsync("BlankReport", CurrentPatientInfo.VisitId);
    }

    private async Task PreviewReportAsync()
    {
        if (CurrentPatientInfo == null) return;

        var allReviewed = PatientTests.All(t =>
            t.ComponentResults.All(c =>
                c.ValidationStatus >= ResultValidationStatus.Reviewed));

        if (!allReviewed)
        {
            _dialogService.ShowWarning(
                "يجب مراجعة جميع النتائج قبل المعاينة.", "تنبيه");
            return;
        }

        await _printService.PrintAsync("CompositeReport", CurrentPatientInfo);
    }

    private async Task SendSmsAsync()
    {
        if (CurrentPatientInfo == null) return;

        if (string.IsNullOrWhiteSpace(CurrentPatientInfo.Phone))
        {
            _dialogService.ShowWarning(
                "لا يوجد رقم هاتف مسجل لهذا المريض.", "تنبيه");
            return;
        }

        var phone = CurrentPatientInfo.Phone;
        var message = $"atient {CurrentPatientInfo.FullNameAr} - Lab ID: {CurrentPatientInfo.VisitCode}\nResults are ready for collection.";

        _dialogService.ShowMessage(
            $"إرسال SMS إلى:\n{phone}\n\n{message}", "إرسال رسالة نصية");

        await System.Threading.Tasks.Task.CompletedTask;
    }
}
