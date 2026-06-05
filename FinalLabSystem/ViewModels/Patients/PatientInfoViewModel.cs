using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class PatientInfoViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private string _patientCode = string.Empty;
    private string? _title;
    private string _fullNameAr = string.Empty;
    private string _sex = "U";
    private string _patientType = "Individual";
    private bool _isVip;
    private double? _approxAge;
    private string _approxAgeUnit = "Years";
    private string? _phone;
    private string? _phone2;
    private string? _address;
    private string? _email;
    private string? _nationalId;
    private string? _notes;

    public PatientInfoViewModel(IPatientService patientService)
    {
        _patientService = patientService;
        TitleSuggestions = new ObservableCollection<string>();
        PatientTypes = new ObservableCollection<string> { "Individual", "Contract", "Company", "Insurance" };
        AgeUnits = new ObservableCollection<string> { "Years", "Months", "Days" };
        _ = LoadTitlesAsync();
    }

    public ObservableCollection<string> TitleSuggestions { get; }

    public ObservableCollection<string> PatientTypes { get; }

    public ObservableCollection<string> AgeUnits { get; }

    public string PatientCode
    {
        get => _patientCode;
        private set => SetProperty(ref _patientCode, value);
    }

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string FullNameAr
    {
        get => _fullNameAr;
        set
        {
            if (SetProperty(ref _fullNameAr, value))
                OnPropertyChanged(nameof(HasErrors));
        }
    }

    public string Sex
    {
        get => _sex;
        set
        {
            if (SetProperty(ref _sex, string.IsNullOrWhiteSpace(value) ? "U" : value))
            {
                OnPropertyChanged(nameof(HasErrors));
                OnPropertyChanged(nameof(IsMale));
                OnPropertyChanged(nameof(IsFemale));
                OnPropertyChanged(nameof(IsUnknownSex));
            }
        }
    }

    public bool IsMale
    {
        get => Sex == "M";
        set
        {
            if (value)
                Sex = "M";
        }
    }

    public bool IsFemale
    {
        get => Sex == "F";
        set
        {
            if (value)
                Sex = "F";
        }
    }

    public bool IsUnknownSex
    {
        get => Sex == "U";
        set
        {
            if (value)
                Sex = "U";
        }
    }

    public string PatientType
    {
        get => _patientType;
        set => SetProperty(ref _patientType, string.IsNullOrWhiteSpace(value) ? "Individual" : value);
    }

    public bool IsVip
    {
        get => _isVip;
        set => SetProperty(ref _isVip, value);
    }

    public double? ApproxAge
    {
        get => _approxAge;
        set => SetProperty(ref _approxAge, value);
    }

    public string ApproxAgeUnit
    {
        get => _approxAgeUnit;
        set => SetProperty(ref _approxAgeUnit, string.IsNullOrWhiteSpace(value) ? "Years" : value);
    }

    public string? Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string? Phone2
    {
        get => _phone2;
        set => SetProperty(ref _phone2, value);
    }

    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string? NationalId
    {
        get => _nationalId;
        set => SetProperty(ref _nationalId, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool HasErrors => string.IsNullOrWhiteSpace(FullNameAr) || !new[] { "M", "F", "U" }.Contains(Sex);

    public async Task GenerateCodeAsync()
    {
        PatientCode = await _patientService.GeneratePatientCodeAsync();
    }

    public void LoadPatient(Patient patient)
    {
        PatientCode = patient.PatientCode;
        Title = patient.Title;
        FullNameAr = patient.FullNameAr;
        Sex = string.IsNullOrWhiteSpace(patient.Sex) ? "U" : patient.Sex;
        PatientType = string.IsNullOrWhiteSpace(patient.PatientType) ? "Individual" : patient.PatientType;
        IsVip = patient.IsVip;
        ApproxAge = patient.ApproxAge;
        ApproxAgeUnit = string.IsNullOrWhiteSpace(patient.ApproxAgeUnit) ? "Years" : patient.ApproxAgeUnit;
        Phone = patient.Phone;
        Phone2 = patient.Phone2;
        Address = patient.Address;
        Email = patient.Email;
        NationalId = patient.NationalId;
        Notes = patient.Notes;
    }

    public Patient ToPatient()
    {
        return new Patient
        {
            PatientCode = PatientCode,
            NationalId = NationalId,
            Title = Title,
            FullNameAr = FullNameAr.Trim(),
            Sex = Sex,
            ApproxAge = ApproxAge,
            ApproxAgeUnit = ApproxAgeUnit,
            Phone = Phone,
            Phone2 = Phone2,
            Address = Address,
            Email = Email,
            Notes = Notes,
            IsVip = IsVip,
            PatientType = PatientType,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task LoadTitlesAsync()
    {
        var titles = await _patientService.GetPatientTitlesAsync();
        TitleSuggestions.Clear();
        foreach (var title in titles)
            TitleSuggestions.Add(title);
    }
}
