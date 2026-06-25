using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class ResultEntryViewModel : ViewModelBase
{
    private readonly IRoutineResultService _routineResultService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IDialogService _dialogService;
    private readonly int _patientId;
    private readonly int _patientAgeDays;
    private readonly string _patientGender;
    private readonly bool _isPregnant;

    private ObservableCollection<TestComponentResultDto> _components;
    private TestComponentResultDto? _selectedComponent;
    private bool _isSaving;

    public ResultEntryViewModel(
        IRoutineResultService routineResultService,
        IVisitService visitService,
        IAuditService auditService,
        ICurrentUserSession currentUserSession,
        IDialogService dialogService,
        int visitTestId,
        int patientId,
        string testTypeName,
        ObservableCollection<TestComponentResultDto> components,
        int patientAgeDays,
        string patientGender,
        bool isPregnant)
    {
        _routineResultService = routineResultService;
        _auditService = auditService;
        _currentUserSession = currentUserSession;
        _dialogService = dialogService;
        _patientId = patientId;
        _patientAgeDays = patientAgeDays;
        _patientGender = patientGender;
        _isPregnant = isPregnant;
        VisitTestId = visitTestId;
        TestTypeName = testTypeName;
        _components = components;

        SaveCommand = new AsyncRelayCommand(async _ => await SaveAsync(), _ => !IsSaving);
        SaveAndReviewCommand = new AsyncRelayCommand(async _ => await SaveAndReviewAsync(), _ => !IsSaving);
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(), _ => !IsSaving);

        foreach (var comp in _components)
        {
            comp.PropertyChanged += OnComponentPropertyChanged;
            SyncResultNumeric(comp);
            RecomputeClinicalStatus(comp);
        }
    }

    public int VisitTestId { get; }
    public string TestTypeName { get; }

    public ObservableCollection<TestComponentResultDto> Components
    {
        get => _components;
        set => SetProperty(ref _components, value);
    }

    public TestComponentResultDto? SelectedComponent
    {
        get => _selectedComponent;
        set => SetProperty(ref _selectedComponent, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand SaveAndReviewCommand { get; }
    public ICommand CancelCommand { get; }

    public object? CustomEditorContent { get; }

    public event EventHandler? SaveCompleted;
    public Action? RequestClose { get; set; }

    private void OnComponentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is TestComponentResultDto comp && e.PropertyName == nameof(TestComponentResultDto.ResultValue))
        {
            SyncResultNumeric(comp);
            RecomputeClinicalStatus(comp);
        }
    }

    private static void SyncResultNumeric(TestComponentResultDto comp)
    {
        if (!string.IsNullOrWhiteSpace(comp.ResultValue)
            && decimal.TryParse(comp.ResultValue, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var parsed))
            comp.ResultNumeric = parsed;
        else
            comp.ResultNumeric = null;
    }

    private void RecomputeClinicalStatus(TestComponentResultDto comp)
    {
        if (!comp.ResultNumeric.HasValue)
        {
            comp.ClinicalStatus = ResultClinicalStatus.Normal;
            return;
        }

        var val = (double)comp.ResultNumeric.Value;

        if (comp.SnapHighCritical.HasValue && val > comp.SnapHighCritical.Value)
        {
            comp.ClinicalStatus = ResultClinicalStatus.Critical;
            return;
        }

        if (comp.SnapLowCritical.HasValue && val < comp.SnapLowCritical.Value)
        {
            comp.ClinicalStatus = ResultClinicalStatus.Critical;
            return;
        }

        if (comp.SnapHighNormal.HasValue && val > comp.SnapHighNormal.Value)
        {
            comp.ClinicalStatus = ResultClinicalStatus.High;
            return;
        }

        if (comp.SnapLowNormal.HasValue && val < comp.SnapLowNormal.Value)
        {
            comp.ClinicalStatus = ResultClinicalStatus.Low;
            return;
        }

        comp.ClinicalStatus = ResultClinicalStatus.Normal;
    }

    private bool HasCriticalValues()
    {
        return Components
            .Any(c => c.IsSelectedForSave &&
                      c.ClinicalStatus == ResultClinicalStatus.Critical &&
                      !string.IsNullOrWhiteSpace(c.ResultValue));
    }

    private bool ConfirmCriticalSave()
    {
        return _dialogService.ShowConfirmation(
            "تحتوي النتائج المحددة على قيم حرجة. هل تريد المتابعة بالحفظ؟",
            "تنبيه: قيم حرجة");
    }

    private async Task SaveAsync()
    {
        IsSaving = true;
        try
        {
            var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
            if (staffId == 0) return;

            if (HasCriticalValues() && !ConfirmCriticalSave())
                return;

            var results = Components
                .Where(c => c.IsSelectedForSave && !string.IsNullOrWhiteSpace(c.ResultValue))
                .Select(c => new TestResult
                {
                    VisitTestId = VisitTestId,
                    ComponentId = c.ComponentId,
                    ResultValue = c.ResultValue,
                    Comment = c.Comment
                })
                .ToList();

            if (results.Count > 0)
            {
                await _routineResultService.SaveNumericOrTextResultsAsync(results, _patientId, staffId);
            }

            SaveCompleted?.Invoke(this, EventArgs.Empty);
            RequestClose?.Invoke();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task SaveAndReviewAsync()
    {
        IsSaving = true;
        try
        {
            var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
            if (staffId == 0) return;

            if (HasCriticalValues() && !ConfirmCriticalSave())
                return;

            var results = Components
                .Where(c => c.IsSelectedForSave && !string.IsNullOrWhiteSpace(c.ResultValue))
                .Select(c => new TestResult
                {
                    VisitTestId = VisitTestId,
                    ComponentId = c.ComponentId,
                    ResultValue = c.ResultValue,
                    Comment = c.Comment
                })
                .ToList();

            if (results.Count > 0)
            {
                await _routineResultService.SaveNumericOrTextResultsAsync(results, _patientId, staffId);
            }

            await _routineResultService.ToggleReviewStatusAsync(VisitTestId, staffId);

            SaveCompleted?.Invoke(this, EventArgs.Empty);
            RequestClose?.Invoke();
        }
        finally
        {
            IsSaving = false;
        }
    }
}
