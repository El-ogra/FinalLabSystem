using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class MedicalHistoryViewModel : ViewModelBase
{
    private bool _isFasting;
    private bool _isPregnant;
    private short? _fastingHours;
    private string? _visitNotes;
    private bool _takenOutsideLab;
    private bool _outsideUrine;
    private bool _outsideStool;
    private bool _outsideBlood;
    private bool _outsideSemen;
    private bool _outsideCsf;
    private bool _hasDiabetes;
    private bool _hasAnemia;
    private bool _hasBleedingDisorder;
    private bool _hasThyroid;
    private bool _hasJointDisease;
    private bool _hasViralInfection;
    private bool _onAnticoagulant;
    private bool _hasHypertension;
    private bool _hasLiverDisease;
    private bool _hasKidneyDisease;
    private bool _hasLupus;
    private bool _hadXrayContrast;

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

    public bool TakenOutsideLab
    {
        get => _takenOutsideLab;
        set => SetProperty(ref _takenOutsideLab, value);
    }

    public bool OutsideUrine
    {
        get => _outsideUrine;
        set => SetProperty(ref _outsideUrine, value);
    }

    public bool OutsideStool
    {
        get => _outsideStool;
        set => SetProperty(ref _outsideStool, value);
    }

    public bool OutsideBlood
    {
        get => _outsideBlood;
        set => SetProperty(ref _outsideBlood, value);
    }

    public bool OutsideSemen
    {
        get => _outsideSemen;
        set => SetProperty(ref _outsideSemen, value);
    }

    public bool OutsideCsf
    {
        get => _outsideCsf;
        set => SetProperty(ref _outsideCsf, value);
    }

    public bool HasDiabetes
    {
        get => _hasDiabetes;
        set => SetProperty(ref _hasDiabetes, value);
    }

    public bool HasAnemia
    {
        get => _hasAnemia;
        set => SetProperty(ref _hasAnemia, value);
    }

    public bool HasBleedingDisorder
    {
        get => _hasBleedingDisorder;
        set => SetProperty(ref _hasBleedingDisorder, value);
    }

    public bool HasThyroid
    {
        get => _hasThyroid;
        set => SetProperty(ref _hasThyroid, value);
    }

    public bool HasJointDisease
    {
        get => _hasJointDisease;
        set => SetProperty(ref _hasJointDisease, value);
    }

    public bool HasViralInfection
    {
        get => _hasViralInfection;
        set => SetProperty(ref _hasViralInfection, value);
    }

    public bool OnAnticoagulant
    {
        get => _onAnticoagulant;
        set => SetProperty(ref _onAnticoagulant, value);
    }

    public bool HasHypertension
    {
        get => _hasHypertension;
        set => SetProperty(ref _hasHypertension, value);
    }

    public bool HasLiverDisease
    {
        get => _hasLiverDisease;
        set => SetProperty(ref _hasLiverDisease, value);
    }

    public bool HasKidneyDisease
    {
        get => _hasKidneyDisease;
        set => SetProperty(ref _hasKidneyDisease, value);
    }

    public bool HasLupus
    {
        get => _hasLupus;
        set => SetProperty(ref _hasLupus, value);
    }

    public bool HadXrayContrast
    {
        get => _hadXrayContrast;
        set => SetProperty(ref _hadXrayContrast, value);
    }

    public ObservableCollection<MedicalHistoryItem> MedicalItems { get; }

    public ObservableCollection<string> SampleTypeFilters { get; }

    public void LoadFromVisit(VisitFullDto dto)
    {
        IsFasting = dto.IsFasting;
        FastingHours = dto.FastingHours;
        IsPregnant = dto.IsPregnant;
        VisitNotes = dto.VisitNotes;
        TakenOutsideLab = dto.TakenOutsideLab;
        OutsideUrine = dto.OutsideUrine;
        OutsideStool = dto.OutsideStool;
        OutsideBlood = dto.OutsideBlood;
        OutsideSemen = dto.OutsideSemen;
        OutsideCsf = dto.OutsideCsf;
        HasDiabetes = dto.HasDiabetes;
        HasAnemia = dto.HasAnemia;
        HasBleedingDisorder = dto.HasBleedingDisorder;
        HasThyroid = dto.HasThyroid;
        HasJointDisease = dto.HasJointDisease;
        HasViralInfection = dto.HasViralInfection;
        OnAnticoagulant = dto.OnAnticoagulant;
        HasHypertension = dto.HasHypertension;
        HasLiverDisease = dto.HasLiverDisease;
        HasKidneyDisease = dto.HasKidneyDisease;
        HasLupus = dto.HasLupus;
        HadXrayContrast = dto.HadXrayContrast;
    }

    public Dictionary<string, bool> ToVisitMedicalData()
    {
        return new Dictionary<string, bool>
        {
            [nameof(TakenOutsideLab)] = TakenOutsideLab,
            [nameof(OutsideUrine)] = OutsideUrine,
            [nameof(OutsideStool)] = OutsideStool,
            [nameof(OutsideBlood)] = OutsideBlood,
            [nameof(OutsideSemen)] = OutsideSemen,
            [nameof(OutsideCsf)] = OutsideCsf,
            [nameof(HasDiabetes)] = HasDiabetes,
            [nameof(HasAnemia)] = HasAnemia,
            [nameof(HasBleedingDisorder)] = HasBleedingDisorder,
            [nameof(HasThyroid)] = HasThyroid,
            [nameof(HasJointDisease)] = HasJointDisease,
            [nameof(HasViralInfection)] = HasViralInfection,
            [nameof(OnAnticoagulant)] = OnAnticoagulant,
            [nameof(HasHypertension)] = HasHypertension,
            [nameof(HasLiverDisease)] = HasLiverDisease,
            [nameof(HasKidneyDisease)] = HasKidneyDisease,
            [nameof(HasLupus)] = HasLupus,
            [nameof(HadXrayContrast)] = HadXrayContrast
        };
    }

    public void ClearAllFields()
    {
        IsFasting = false;
        FastingHours = null;
        IsPregnant = false;
        VisitNotes = null;
        TakenOutsideLab = false;
        OutsideUrine = false;
        OutsideStool = false;
        OutsideBlood = false;
        OutsideSemen = false;
        OutsideCsf = false;
        HasDiabetes = false;
        HasAnemia = false;
        HasBleedingDisorder = false;
        HasThyroid = false;
        HasJointDisease = false;
        HasViralInfection = false;
        OnAnticoagulant = false;
        HasHypertension = false;
        HasLiverDisease = false;
        HasKidneyDisease = false;
        HasLupus = false;
        HadXrayContrast = false;

        foreach (var item in MedicalItems)
        {
            item.IsChecked = false;
            item.Description = string.Empty;
        }
    }

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
