using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class ResultEntryViewModel : ViewModelBase
{
    private readonly IRoutineResultService _routineResultService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly int _patientId;

    private ObservableCollection<TestComponentResultDto> _components;
    private bool _isSaving;

    public ResultEntryViewModel(
        IRoutineResultService routineResultService,
        IVisitService visitService,
        IAuditService auditService,
        ICurrentUserSession currentUserSession,
        int visitTestId,
        int patientId,
        string testTypeName,
        ObservableCollection<TestComponentResultDto> components)
    {
        _routineResultService = routineResultService;
        _auditService = auditService;
        _currentUserSession = currentUserSession;
        _patientId = patientId;
        VisitTestId = visitTestId;
        TestTypeName = testTypeName;
        _components = components;

        SaveCommand = new AsyncRelayCommand(async _ => await SaveAsync(), _ => !IsSaving);
        CancelCommand = new RelayCommand(_ => { });
    }

    public int VisitTestId { get; }
    public string TestTypeName { get; }

    public ObservableCollection<TestComponentResultDto> Components
    {
        get => _components;
        set => SetProperty(ref _components, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler? SaveCompleted;

    private async Task SaveAsync()
    {
        IsSaving = true;
        try
        {
            var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
            if (staffId == 0) return;

            var results = Components
                .Where(c => !string.IsNullOrWhiteSpace(c.ResultValue))
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
        }
        finally
        {
            IsSaving = false;
        }
    }
}
