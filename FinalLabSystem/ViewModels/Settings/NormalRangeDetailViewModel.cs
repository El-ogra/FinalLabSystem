using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public enum NormalRangeFor { Female, Male, Both }

public enum RangeSex { M, F, B }

public sealed class NormalRangeDetailViewModel : ViewModelBase
{
    private readonly ITestCatalogService _testCatalogService;
    private NormalRange _editableRange = CreateEmptyRange();
    private bool _isDirty;
    private NormalRange? _lastLoadedRange;
    private string? _lastLoadedUnit;
    private NormalRangeFor _rangeFor;
    private RangeSex _sex;
    private bool _forPregnantOnly;
    private ICommand? _saveCommand;
    private ICommand? _cancelCommand;

    public string[] AgeUnitOptions { get; } = new[] { "Days", "Months", "Years" };

    public NormalRangeDetailViewModel(ITestCatalogService testCatalogService)
    {
        _testCatalogService = testCatalogService;
    }

    public NormalRange EditableRange
    {
        get => _editableRange;
        private set => SetProperty(ref _editableRange, value);
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    public NormalRangeFor RangeFor
    {
        get => _rangeFor;
        set
        {
            if (SetProperty(ref _rangeFor, value))
            {
                MarkDirty();
                OnPropertyChanged(nameof(IsSexEnabled));
                OnPropertyChanged(nameof(IsAgeEnabled));
                OnPropertyChanged(nameof(IsRangeForAll));
                OnPropertyChanged(nameof(IsRangeForSexAndAge));
                OnPropertyChanged(nameof(IsRangeForSexOnly));
                OnPropertyChanged(nameof(IsRangeForAgeOnly));
            }
        }
    }

    public bool IsSexEnabled => RangeFor != NormalRangeFor.Both;

    public bool IsAgeEnabled => RangeFor == NormalRangeFor.Both;

    public bool IsRangeForAll
    {
        get => RangeFor == NormalRangeFor.Both;
        set { if (value) RangeFor = NormalRangeFor.Both; }
    }

    public bool IsRangeForSexAndAge
    {
        get => RangeFor == NormalRangeFor.Female;
        set { if (value) RangeFor = NormalRangeFor.Female; }
    }

    public bool IsRangeForSexOnly
    {
        get => RangeFor == NormalRangeFor.Male;
        set { if (value) RangeFor = NormalRangeFor.Male; }
    }

    public bool IsRangeForAgeOnly
    {
        get => RangeFor == NormalRangeFor.Both;
        set { if (value) RangeFor = NormalRangeFor.Both; }
    }

    public RangeSex Sex
    {
        get => _sex;
        set
        {
            if (SetProperty(ref _sex, value))
                MarkDirty();
        }
    }

    public bool ForPregnantOnly
    {
        get => _forPregnantOnly;
        set
        {
            if (SetProperty(ref _forPregnantOnly, value))
                MarkDirty();
        }
    }

    public int AgeFromDays
    {
        get => EditableRange.AgeFromDays;
        set => SetRangeProperty(EditableRange.AgeFromDays, value, v => EditableRange.AgeFromDays = v);
    }

    public int AgeToDays
    {
        get => EditableRange.AgeToDays;
        set => SetRangeProperty(EditableRange.AgeToDays, value, v => EditableRange.AgeToDays = v);
    }

    public string? AgeDescription
    {
        get => EditableRange.AgeDescription;
        set => SetRangeProperty(EditableRange.AgeDescription, value, v => EditableRange.AgeDescription = v);
    }

    public string FastingState
    {
        get => EditableRange.FastingState;
        set => SetRangeProperty(EditableRange.FastingState, value, v => EditableRange.FastingState = v);
    }

    public double? LowNormal
    {
        get => EditableRange.LowNormal;
        set => SetRangeProperty(EditableRange.LowNormal, value, v => EditableRange.LowNormal = value);
    }

    public double? HighNormal
    {
        get => EditableRange.HighNormal;
        set => SetRangeProperty(EditableRange.HighNormal, value, v => EditableRange.HighNormal = value);
    }

    public double? LowCritical
    {
        get => EditableRange.LowCritical;
        set => SetRangeProperty(EditableRange.LowCritical, value, v => EditableRange.LowCritical = value);
    }

    public double? HighCritical
    {
        get => EditableRange.HighCritical;
        set => SetRangeProperty(EditableRange.HighCritical, value, v => EditableRange.HighCritical = value);
    }

    public string? NormalRangeText
    {
        get => EditableRange.NormalRangeText;
        set => SetRangeProperty(EditableRange.NormalRangeText, value, v => EditableRange.NormalRangeText = v);
    }

    public string? RangeNote
    {
        get => EditableRange.RangeNote;
        set => SetRangeProperty(EditableRange.RangeNote, value, v => EditableRange.RangeNote = v);
    }

    public string? AgeUnit
    {
        get => EditableRange.AgeUnit;
        set => SetRangeProperty(EditableRange.AgeUnit, value, v => EditableRange.AgeUnit = v);
    }

    public string? LowFlag
    {
        get => EditableRange.LowFlag;
        set => SetRangeProperty(EditableRange.LowFlag, value, v => EditableRange.LowFlag = v);
    }

    public string? HighFlag
    {
        get => EditableRange.HighFlag;
        set => SetRangeProperty(EditableRange.HighFlag, value, v => EditableRange.HighFlag = v);
    }

    public string? LowComment
    {
        get => EditableRange.LowComment;
        set => SetRangeProperty(EditableRange.LowComment, value, v => EditableRange.LowComment = v);
    }

    public string? HighComment
    {
        get => EditableRange.HighComment;
        set => SetRangeProperty(EditableRange.HighComment, value, v => EditableRange.HighComment = v);
    }

    public string? CriticalRangeText
    {
        get => EditableRange.CriticalRangeText;
        set => SetRangeProperty(EditableRange.CriticalRangeText, value, v => EditableRange.CriticalRangeText = v);
    }

    public string? CriticalFlag
    {
        get => EditableRange.CriticalFlag;
        set => SetRangeProperty(EditableRange.CriticalFlag, value, v => EditableRange.CriticalFlag = v);
    }

    public string? CriticalComment
    {
        get => EditableRange.CriticalComment;
        set => SetRangeProperty(EditableRange.CriticalComment, value, v => EditableRange.CriticalComment = v);
    }

    public bool IsSexMale
    {
        get => Sex == RangeSex.M;
        set { if (value) Sex = RangeSex.M; }
    }

    public bool IsSexFemale
    {
        get => Sex == RangeSex.F;
        set { if (value) Sex = RangeSex.F; }
    }

    public bool IsSexBoth
    {
        get => Sex == RangeSex.B;
        set { if (value) Sex = RangeSex.B; }
    }

    public bool IsFastingAny
    {
        get => FastingState == "Any";
        set { if (value) FastingState = "Any"; }
    }

    public bool IsFasting
    {
        get => FastingState == "Fasting";
        set { if (value) FastingState = "Fasting"; }
    }

    public ICommand SaveCommand => _saveCommand ??= new RelayCommand(async _ => await SaveAsync(), _ => IsDirty);

    public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(_ => Cancel());

    public void Load(NormalRange? range, string? unit)
    {
        _lastLoadedRange = range is null ? null : CloneRange(range);
        _lastLoadedUnit = unit;

        EditableRange = range is null ? CreateEmptyRange() : CloneRange(range);

        Sex = EditableRange.Sex switch
        {
            "M" => RangeSex.M,
            "F" => RangeSex.F,
            _ => RangeSex.B
        };

        RangeFor = EditableRange.Sex switch
        {
            "M" => NormalRangeFor.Male,
            "F" => NormalRangeFor.Female,
            _ => NormalRangeFor.Both
        };

        ForPregnantOnly = EditableRange.ForPregnantOnly ?? false;

        RaiseAllChanged();
        IsDirty = false;
    }

    private async Task SaveAsync()
    {
        if (LowNormal.HasValue && HighNormal.HasValue && LowNormal.Value > HighNormal.Value)
        {
            MessageBox.Show("Low Normal يجب أن يكون أقل من أو يساوي High Normal.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (LowCritical.HasValue && HighCritical.HasValue && LowCritical.Value > HighCritical.Value)
        {
            MessageBox.Show("Low Critical يجب أن يكون أقل من أو يساوي High Critical.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        EditableRange.Sex = Sex switch
        {
            RangeSex.M => "M",
            RangeSex.F => "F",
            RangeSex.B => "Both",
            _ => "Both"
        };
        EditableRange.ForPregnantOnly = ForPregnantOnly;

        var saved = await _testCatalogService.SaveRangeAsync(EditableRange);
        _lastLoadedRange = CloneRange(saved);
        IsDirty = false;
    }

    private void Cancel()
    {
        if (_lastLoadedRange is not null)
            Load(_lastLoadedRange, _lastLoadedUnit);
    }

    private void MarkDirty()
    {
        IsDirty = true;
    }

    private void SetRangeProperty<T>(T oldValue, T newValue, Action<T> assign)
    {
        if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

        assign(newValue);
        RaiseAllChanged();
        IsDirty = true;
    }

    private void RaiseAllChanged()
    {
        OnPropertyChanged(nameof(RangeFor));
        OnPropertyChanged(nameof(IsSexEnabled));
        OnPropertyChanged(nameof(IsAgeEnabled));
        OnPropertyChanged(nameof(IsRangeForAll));
        OnPropertyChanged(nameof(IsRangeForSexAndAge));
        OnPropertyChanged(nameof(IsRangeForSexOnly));
        OnPropertyChanged(nameof(IsRangeForAgeOnly));
        OnPropertyChanged(nameof(Sex));
        OnPropertyChanged(nameof(ForPregnantOnly));
        OnPropertyChanged(nameof(AgeFromDays));
        OnPropertyChanged(nameof(AgeToDays));
        OnPropertyChanged(nameof(AgeDescription));
        OnPropertyChanged(nameof(FastingState));
        OnPropertyChanged(nameof(LowNormal));
        OnPropertyChanged(nameof(HighNormal));
        OnPropertyChanged(nameof(LowCritical));
        OnPropertyChanged(nameof(HighCritical));
        OnPropertyChanged(nameof(NormalRangeText));
        OnPropertyChanged(nameof(RangeNote));
        OnPropertyChanged(nameof(AgeUnit));
        OnPropertyChanged(nameof(LowFlag));
        OnPropertyChanged(nameof(HighFlag));
        OnPropertyChanged(nameof(LowComment));
        OnPropertyChanged(nameof(HighComment));
        OnPropertyChanged(nameof(CriticalRangeText));
        OnPropertyChanged(nameof(CriticalFlag));
        OnPropertyChanged(nameof(CriticalComment));
        OnPropertyChanged(nameof(IsSexMale));
        OnPropertyChanged(nameof(IsSexFemale));
        OnPropertyChanged(nameof(IsSexBoth));
        OnPropertyChanged(nameof(IsFastingAny));
        OnPropertyChanged(nameof(IsFasting));
    }

    private static NormalRange CreateEmptyRange()
    {
        return new NormalRange
        {
            Sex = "Both",
            FastingState = "Any",
            AgeFromDays = 0,
            AgeToDays = 36500,
            AgeUnit = "Days"
        };
    }

    private static NormalRange CloneRange(NormalRange range)
    {
        return new NormalRange
        {
            RangeId = range.RangeId,
            ComponentId = range.ComponentId,
            Sex = range.Sex,
            AgeFromDays = range.AgeFromDays,
            AgeToDays = range.AgeToDays,
            AgeDescription = range.AgeDescription,
            ForPregnantOnly = range.ForPregnantOnly,
            AgeUnit = range.AgeUnit,
            LowFlag = range.LowFlag,
            HighFlag = range.HighFlag,
            LowComment = range.LowComment,
            HighComment = range.HighComment,
            CriticalRangeText = range.CriticalRangeText,
            CriticalFlag = range.CriticalFlag,
            CriticalComment = range.CriticalComment,
            FastingState = range.FastingState,
            LowNormal = range.LowNormal,
            HighNormal = range.HighNormal,
            LowCritical = range.LowCritical,
            HighCritical = range.HighCritical,
            NormalRangeText = range.NormalRangeText,
            RangeNote = range.RangeNote
        };
    }
}
