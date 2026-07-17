using System.Collections.ObjectModel;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Text;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class PatientInfoViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IPatientService _patientService;
    private string _patientCode = string.Empty;
    private string? _title;
    private string _fullNameAr = string.Empty;
    private string _sex = "U";
    private string _patientType = "Individual";
    private BillingType _selectedBillingType = BillingType.Individual;
    private bool _isVip;
    private decimal? _approxAgeValue;
    private string _approxAgeUnit = "Years";
    private string? _phone;
    private string? _phone2;
    private string? _address;
    private string? _email;
    private string? _nationalId;
    private string? _labId;
    private string? _notes;

    public PatientInfoViewModel(IPatientService patientService)
    {
        _patientService = patientService;
        TitleSuggestions = new ObservableCollection<string>();
        PatientTypes = new ObservableCollection<string> { "Individual", "Contract", "Company", "Insurance" };
        AgeUnits = new ObservableCollection<string> { "Years", "Months", "Days" };
    }

    public async Task InitializeAsync()
    {
        try
        {
            var titles = await _patientService.GetPatientTitlesAsync();
            TitleSuggestions.Clear();
            foreach (var title in titles)
                TitleSuggestions.Add(title);
        }
        catch
        {
            // TODO F-07: _dialogService.ShowError("حدث خطأ أثناء تحميل البيانات.");
        }
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
                SuggestTitleForSex(_sex);
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

    public BillingType SelectedBillingType
    {
        get => _selectedBillingType;
        set => SetProperty(ref _selectedBillingType, value);
    }

    public ObservableCollection<BillingType> BillingTypes { get; } =
        new(Enum.GetValues<BillingType>());

    public bool IsVip
    {
        get => _isVip;
        set => SetProperty(ref _isVip, value);
    }

    public decimal? ApproxAgeValue
    {
        get => _approxAgeValue;
        set
        {
            if (SetProperty(ref _approxAgeValue, value))
                OnPropertyChanged(nameof(HasErrors));
        }
    }

    public string ApproxAgeUnit
    {
        get => _approxAgeUnit;
        set
        {
            if (SetProperty(ref _approxAgeUnit, string.IsNullOrWhiteSpace(value) ? "Years" : value))
                OnPropertyChanged(nameof(HasErrors));
        }
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

    public string? LabId
    {
        get => _labId;
        set => SetProperty(ref _labId, value);
    }

    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public new bool HasErrors => string.IsNullOrWhiteSpace(FullNameAr)
        || !new[] { "M", "F", "U" }.Contains(Sex)
        || !IsAgeValid();

    private bool IsAgeValid()
    {
        if (ApproxAgeValue is null || ApproxAgeValue <= 0)
            return false;

        return ApproxAgeUnit switch
        {
            "Years" => true,
            "Months" => ApproxAgeValue >= 1 && ApproxAgeValue <= 11,
            "Days" => ApproxAgeValue >= 1 && ApproxAgeValue <= 29,
            _ => false
        };
    }

    /// <summary>
    /// يُحوّل قيمة السن إلى صيغة التخزين في قاعدة البيانات.
    /// إذا الوحدة = Years والقيمة تحتوي كسراً (مثال: 2.5) → يحوّل إلى شهور (30 شهر).
    /// </summary>
    public (int? Age, string Unit) NormalizeAgeToStorage()
    {
        if (ApproxAgeValue is null || ApproxAgeValue <= 0)
            return (null, "Years");

        if (ApproxAgeUnit == "Years" && ApproxAgeValue % 1 != 0)
        {
            var totalMonths = (int)(ApproxAgeValue * 12);
            return (totalMonths, "Months");
        }

        return ApproxAgeUnit switch
        {
            "Years" => ((int?)ApproxAgeValue, "Years"),
            "Months" => ((int?)ApproxAgeValue, "Months"),
            "Days" => ((int?)ApproxAgeValue, "Days"),
            _ => ((int?)ApproxAgeValue, "Years")
        };
    }

    public async Task GenerateCodeAsync()
    {
        PatientCode = await _patientService.GeneratePatientCodeAsync();
    }

    private static readonly Dictionary<string, string> DefaultTitlesBySex = new()
    {
        ["M"] = "السيد",
        ["F"] = "السيدة",
        ["U"] = ""
    };

    private string _lastAutoTitle = "";

    private async void SuggestTitleForSex(string sex)
    {
        var defaultTitle = DefaultTitlesBySex.GetValueOrDefault(sex, "");

        if (string.IsNullOrEmpty(Title) || Title == _lastAutoTitle)
        {
            Title = defaultTitle;
            _lastAutoTitle = defaultTitle;
        }

        try
        {
            var titles = await _patientService.GetPatientTitlesBySexAsync(sex);
            TitleSuggestions.Clear();
            foreach (var title in titles)
                TitleSuggestions.Add(title);
        }
        catch
        {
            // TODO: _dialogService.ShowError("حدث خطأ أثناء تحميل الألقاب.");
        }
    }

    public void LoadPatient(Patient patient)
    {
        PatientCode = patient.PatientCode;
        Title = patient.Title;
        FullNameAr = patient.FullNameAr;
        Sex = string.IsNullOrWhiteSpace(patient.Sex) ? "U" : patient.Sex;
        PatientType = string.IsNullOrWhiteSpace(patient.PatientType) ? "Individual" : patient.PatientType;
        IsVip = patient.IsVip;
        ApproxAgeValue = patient.ApproxAge;
        ApproxAgeUnit = string.IsNullOrWhiteSpace(patient.ApproxAgeUnit) ? "Years" : patient.ApproxAgeUnit;
        Phone = patient.Phone;
        Phone2 = patient.Phone2;
        Address = patient.Address;
        Email = patient.Email;
        NationalId = patient.NationalId;
        LabId = patient.LabId;
        Notes = patient.Notes;
    }

    public void LoadPatient(VisitFullDto dto)
    {
        PatientCode = dto.PatientCode;
        Title = dto.Title;
        FullNameAr = dto.FullNameAr;
        Sex = string.IsNullOrWhiteSpace(dto.Sex) ? "U" : dto.Sex;
        PatientType = string.IsNullOrWhiteSpace(dto.PatientType) ? "Individual" : dto.PatientType;
        SelectedBillingType = dto.BillingType;
        IsVip = dto.IsVip;
        ApproxAgeValue = dto.ApproxAge;
        ApproxAgeUnit = string.IsNullOrWhiteSpace(dto.ApproxAgeUnit) ? "Years" : dto.ApproxAgeUnit;
        Phone = dto.Phone;
        Phone2 = dto.Phone2;
        Address = dto.Address;
        Email = dto.Email;
        NationalId = dto.NationalId;
        LabId = dto.LabId;
        Notes = dto.Notes;
    }

    public void ClearAllFields()
    {
        PatientCode = string.Empty;
        Title = null;
        FullNameAr = string.Empty;
        Sex = "U";
        PatientType = "Individual";
        SelectedBillingType = BillingType.Individual;
        IsVip = false;
        ApproxAgeValue = null;
        ApproxAgeUnit = "Years";
        Phone = null;
        Phone2 = null;
        Address = null;
        Email = null;
        NationalId = null;
        LabId = null;
        Notes = null;
    }

    public Patient ToPatient()
    {
        var (storageAge, storageUnit) = NormalizeAgeToStorage();
        return new Patient
        {
            PatientCode = PatientCode,
            NationalId = NationalId,
            Title = ArabicTextNormalizer.Normalize(Title ?? ""),
            FullNameAr = ArabicTextNormalizer.Normalize(FullNameAr),
            Sex = Sex,
            ApproxAge = storageAge,
            ApproxAgeUnit = storageUnit,
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


}
