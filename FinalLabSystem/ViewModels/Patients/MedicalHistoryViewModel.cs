using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class MedicalHistoryViewModel : ViewModelBase
{
    private bool _isFasting;
    private bool _isPregnant;
    private short? _fastingHours;
    private string? _visitNotes;

    public MedicalHistoryViewModel()
    {
        MedicalItems = new ObservableCollection<MedicalHistoryItem>
        {
            new("DISEASE", string.Empty),
            new("MEDICATION", string.Empty),
            new("ALLERGY", string.Empty),
            new("SURGERY", string.Empty),
            new("OTHER", string.Empty)
        };

        SampleTypeFilters = new ObservableCollection<string> { "Blood", "Urine", "Stool", "Semen", "CSF" };
    }

    public bool IsFasting
    {
        get => _isFasting;
        set => SetProperty(ref _isFasting, value);
    }

    public bool IsPregnant
    {
        get => _isPregnant;
        set => SetProperty(ref _isPregnant, value);
    }

    public short? FastingHours
    {
        get => _fastingHours;
        set => SetProperty(ref _fastingHours, value);
    }

    public string? VisitNotes
    {
        get => _visitNotes;
        set => SetProperty(ref _visitNotes, value);
    }

    public ObservableCollection<MedicalHistoryItem> MedicalItems { get; }

    public ObservableCollection<string> SampleTypeFilters { get; }

    public List<PatientMedicalHistory> ToMedicalHistoryList()
    {
        return MedicalItems
            .Where(item => item.IsChecked)
            .Select(item => new PatientMedicalHistory
            {
                HistoryType = item.HistoryType,
                Description = string.IsNullOrWhiteSpace(item.Description) ? item.HistoryType : item.Description.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();
    }
}

public sealed class MedicalHistoryItem : ViewModelBase
{
    private string _description;
    private bool _isChecked;

    public MedicalHistoryItem(string historyType, string description)
    {
        HistoryType = historyType;
        _description = description;
    }

    public string HistoryType { get; }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
}
