using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class TodayPatientsDialogViewModel : ViewModelBase
{
    private readonly IVisitService _visitService;
    private string _searchText = string.Empty;
    private TodayPatientWithStatusDto? _selectedPatient;

    public TodayPatientsDialogViewModel(IVisitService visitService)
    {
        _visitService = visitService;
        PatientsView = CollectionViewSource.GetDefaultView(AllPatients);
        PatientsView.Filter = FilterPatient;

        SelectCommand = new RelayCommand(_ => Select());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public ObservableCollection<TodayPatientWithStatusDto> AllPatients { get; } = new();

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
        set => SetProperty(ref _selectedPatient, value);
    }

    public ICommand SelectCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<bool>? CloseRequested;

    public async Task LoadAsync()
    {
        AllPatients.Clear();
        var patients = await _visitService.GetTodayPatientsWithStatusAsync();
        foreach (var patient in patients)
            AllPatients.Add(patient);
    }

    private bool FilterPatient(object item)
    {
        if (item is not TodayPatientWithStatusDto patient)
            return false;

        var term = SearchText?.Trim();
        if (string.IsNullOrWhiteSpace(term))
            return true;

        return patient.PatientCode.Contains(term, StringComparison.OrdinalIgnoreCase)
            || patient.FullNameAr.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private void Select()
    {
        if (SelectedPatient is null)
            return;

        CloseRequested?.Invoke(this, true);
    }

    private void Cancel()
    {
        CloseRequested?.Invoke(this, false);
    }
}
