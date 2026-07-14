using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class CultureEntryViewModel : ViewModelBase
{
    private readonly ICultureResultService _cultureService;
    private readonly IDialogService _dialogService;
    private readonly FinalLabDbContext _context;
    private readonly int _visitTestId;
    private readonly int _patientId;
    private readonly bool _isPregnant;
    private readonly int _patientAgeDays;

    private string _specimenSource = string.Empty;
    private string _cultureCondition = string.Empty;
    private string _colonyCount = string.Empty;
    private short? _incubationHours = 48;
    private DateTime? _receivedAt;
    private DateTime? _finalReadingAt;
    private string _finalComment = string.Empty;
    private string _cultureResult = string.Empty;
    private OrganismInputVm? _selectedOrganism;
    private bool _isSaving;

    public CultureEntryViewModel(
        ICultureResultService cultureService,
        IDialogService dialogService,
        FinalLabDbContext context,
        int visitTestId,
        int patientId,
        bool isPregnant,
        int patientAgeDays)
    {
        _cultureService = cultureService;
        _dialogService = dialogService;
        _context = context;
        _visitTestId = visitTestId;
        _patientId = patientId;
        _isPregnant = isPregnant;
        _patientAgeDays = patientAgeDays;

        Organisms = new ObservableCollection<OrganismInputVm>();
        AvailableAntibiotics = new ObservableCollection<AntibioticCatalog>();

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsSaving);
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        AddOrganismCommand = new AsyncRelayCommand(AddOrganismAsync, () => Organisms.Count < 3);
        RemoveOrganismCommand = new AsyncRelayCommand<OrganismInputVm>(RemoveOrganismAsync);
    }

    public ObservableCollection<OrganismInputVm> Organisms { get; }
    public ObservableCollection<AntibioticCatalog> AvailableAntibiotics { get; }

    public string SpecimenSource
    {
        get => _specimenSource;
        set => SetProperty(ref _specimenSource, value);
    }

    public string CultureCondition
    {
        get => _cultureCondition;
        set => SetProperty(ref _cultureCondition, value);
    }

    public string ColonyCount
    {
        get => _colonyCount;
        set => SetProperty(ref _colonyCount, value);
    }

    public short? IncubationHours
    {
        get => _incubationHours;
        set => SetProperty(ref _incubationHours, value);
    }

    public DateTime? ReceivedAt
    {
        get => _receivedAt;
        set => SetProperty(ref _receivedAt, value);
    }

    public DateTime? FinalReadingAt
    {
        get => _finalReadingAt;
        set => SetProperty(ref _finalReadingAt, value);
    }

    public string FinalComment
    {
        get => _finalComment;
        set => SetProperty(ref _finalComment, value);
    }

    public string CultureResult
    {
        get => _cultureResult;
        set
        {
            if (SetProperty(ref _cultureResult, value))
                OnPropertyChanged(nameof(CanSaveWithoutOrganisms));
        }
    }

    public OrganismInputVm? SelectedOrganism
    {
        get => _selectedOrganism;
        set => SetProperty(ref _selectedOrganism, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    public bool CanSaveWithoutOrganisms =>
        CultureResult.Trim().Equals("No Growth", StringComparison.OrdinalIgnoreCase);

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AddOrganismCommand { get; }
    public ICommand RemoveOrganismCommand { get; }

    public Action? RequestClose { get; set; }

    public async Task LoadAsync()
    {
        var existing = await _cultureService.GetByVisitTestIdAsync(_visitTestId);
        if (existing != null)
        {
            CultureResult = existing.CultureResult;
            SpecimenSource = existing.SpecimenSource ?? string.Empty;
            CultureCondition = existing.CultureCondition ?? string.Empty;
            ColonyCount = existing.ColonyCount ?? string.Empty;
            IncubationHours = existing.IncubationHours;
            ReceivedAt = existing.ReceivedAt;
            FinalReadingAt = existing.FinalReadingAt;
            FinalComment = existing.FinalComment ?? string.Empty;

            foreach (var org in existing.MicrobiologyOrganisms.OrderBy(o => o.SortOrder))
            {
                var vm = new OrganismInputVm
                {
                    OrganismId = org.OrganismId,
                    OrganismName = org.OrganismName,
                    GramStain = org.GramStain ?? string.Empty,
                    ColonyCount = org.ColonyCount ?? string.Empty,
                    Morphology = org.Morphology ?? string.Empty,
                    SortOrder = org.SortOrder
                };

                foreach (var abx in org.OrganismAntibiotics)
                {
                    vm.AntibioticResults.Add(new AntibioticResultVm
                    {
                        AntibioticResultId = abx.AntibioticResultId,
                        AntibioticName = abx.AntibioticName,
                        Sensitivity = abx.Sensitivity,
                        AntibioticCatalogId = abx.AntibioticCatalogId
                    });
                }

                Organisms.Add(vm);
            }
        }
        else
        {
            ReceivedAt = DateTime.Now;
            CultureResult = "PENDING";
        }

        await LoadAvailableAntibioticsAsync();
    }

    private async Task LoadAvailableAntibioticsAsync()
    {
        var antibiotics = await _cultureService.GetSafeAntibioticsAsync(_isPregnant, _patientAgeDays < 12 * 365);
        AvailableAntibiotics.Clear();
        foreach (var abx in antibiotics)
            AvailableAntibiotics.Add(abx);
    }

    private async Task AddOrganismAsync()
    {
        if (Organisms.Count >= 3)
        {
            _dialogService.ShowWarning("لا يمكن إضافة أكثر من 3 كائنات حية.", "حد أقصى");
            return;
        }

        var newOrg = new OrganismInputVm
        {
            SortOrder = (byte)(Organisms.Count + 1)
        };

        foreach (var abx in AvailableAntibiotics)
        {
            newOrg.AntibioticResults.Add(new AntibioticResultVm
            {
                AntibioticName = abx.AntibioticName,
                Sensitivity = AntibioticSensitivity.ResistantFor,
                AntibioticCatalogId = abx.AntibioticId
            });
        }

        Organisms.Add(newOrg);
        SelectedOrganism = newOrg;

        await Task.CompletedTask;
    }

    private async Task RemoveOrganismAsync(OrganismInputVm? organism)
    {
        if (organism == null) return;
        Organisms.Remove(organism);

        for (int i = 0; i < Organisms.Count; i++)
            Organisms[i].SortOrder = (byte)(i + 1);

        if (SelectedOrganism == organism)
            SelectedOrganism = Organisms.FirstOrDefault();

        await Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        if (IsSaving) return;

        if (Organisms.Count == 0 && !CanSaveWithoutOrganisms)
        {
            _dialogService.ShowWarning(
                "يجب إدخال كائن حي واحد على الأقل، أو تعيين النتيجة إلى 'No Growth'.",
                "تحقق");
            return;
        }

        foreach (var org in Organisms)
        {
            if (string.IsNullOrWhiteSpace(org.OrganismName))
            {
                _dialogService.ShowWarning(
                    $"يجب إدخال اسم الكائن في الصف رقم {org.SortOrder}.",
                    "تحقق");
                return;
            }
        }

        IsSaving = true;
        try
        {
            var culture = new MicrobiologyCulture
            {
                CultureId = 0,
                VisitTestId = _visitTestId,
                CultureResult = CultureResult,
                SpecimenSource = SpecimenSource,
                CultureCondition = CultureCondition,
                ColonyCount = ColonyCount,
                IncubationHours = IncubationHours,
                ReceivedAt = ReceivedAt,
                FinalReadingAt = FinalReadingAt,
                FinalComment = FinalComment
            };

            var organisms = new List<MicrobiologyOrganism>();
            foreach (var orgVm in Organisms)
            {
                var organism = new MicrobiologyOrganism
                {
                    OrganismId = orgVm.OrganismId,
                    OrganismName = orgVm.OrganismName,
                    GramStain = orgVm.GramStain,
                    ColonyCount = orgVm.ColonyCount,
                    Morphology = orgVm.Morphology,
                    SortOrder = orgVm.SortOrder,
                    OrganismAntibiotics = orgVm.AntibioticResults
                        .Select(r => new OrganismAntibiotic
                        {
                            AntibioticResultId = r.AntibioticResultId,
                            AntibioticName = r.AntibioticName,
                            Sensitivity = r.Sensitivity,
                            AntibioticCatalogId = r.AntibioticCatalogId
                        }).ToList()
                };
                organisms.Add(organism);
            }

            await _cultureService.SaveFullCultureAsync(culture, organisms);
            _dialogService.ShowMessage("تم حفظ نتيجة المزرعة بنجاح.", "حفظ");
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"حدث خطأ أثناء الحفظ: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }
}

public sealed class OrganismInputVm : INotifyPropertyChanged
{
    private string _organismName = string.Empty;
    private string _gramStain = string.Empty;
    private string _colonyCount = string.Empty;
    private string _morphology = string.Empty;

    public int OrganismId { get; set; }
    public byte SortOrder { get; set; }

    public string OrganismName
    {
        get => _organismName;
        set { _organismName = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OrganismName))); }
    }

    public string GramStain
    {
        get => _gramStain;
        set { _gramStain = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GramStain))); }
    }

    public string ColonyCount
    {
        get => _colonyCount;
        set { _colonyCount = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColonyCount))); }
    }

    public string Morphology
    {
        get => _morphology;
        set { _morphology = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Morphology))); }
    }

    public ObservableCollection<AntibioticResultVm> AntibioticResults { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class AntibioticResultVm : INotifyPropertyChanged
{
    private AntibioticSensitivity _sensitivity = AntibioticSensitivity.ResistantFor;

    public int AntibioticResultId { get; set; }
    public string AntibioticName { get; set; } = string.Empty;
    public int? AntibioticCatalogId { get; set; }

    public AntibioticSensitivity Sensitivity
    {
        get => _sensitivity;
        set
        {
            _sensitivity = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sensitivity)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
