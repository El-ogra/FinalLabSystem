using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class NormalRangeDetailViewModel : ViewModelBase
{
    private NormalRange _editableRange = CreateEmptyRange();
    private bool _isDirty;
    private string? _unit;

    public NormalRangeDetailViewModel()
    {
        ApplyCommand = new RelayCommand(_ => Apply(), _ => EditableRange.ComponentId >= 0);
    }

    public event EventHandler<NormalRange>? RangeApplied;

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

    public string Sex
    {
        get => EditableRange.Sex;
        set => SetRangeProperty(EditableRange.Sex, value, v => EditableRange.Sex = v);
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

    public bool? AppliesToPregnant
    {
        get => EditableRange.AppliesToPregnant;
        set => SetRangeProperty(EditableRange.AppliesToPregnant, value, v => EditableRange.AppliesToPregnant = v);
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

    public string? Unit
    {
        get => _unit;
        set
        {
            if (SetProperty(ref _unit, value))
                IsDirty = true;
        }
    }

    public bool IsSexMale
    {
        get => Sex == "M";
        set { if (value) Sex = "M"; }
    }

    public bool IsSexFemale
    {
        get => Sex == "F";
        set { if (value) Sex = "F"; }
    }

    public bool IsSexBoth
    {
        get => Sex == "Both";
        set { if (value) Sex = "Both"; }
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

    public ICommand ApplyCommand { get; }

    public void Load(NormalRange? range, string? unit)
    {
        EditableRange = range is null ? CreateEmptyRange() : CloneRange(range);
        Unit = unit;
        RaiseAllChanged();
        IsDirty = false;
    }

    private void Apply()
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

        RangeApplied?.Invoke(this, CloneRange(EditableRange));
        IsDirty = false;
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
        OnPropertyChanged(nameof(Sex));
        OnPropertyChanged(nameof(AgeFromDays));
        OnPropertyChanged(nameof(AgeToDays));
        OnPropertyChanged(nameof(AgeDescription));
        OnPropertyChanged(nameof(FastingState));
        OnPropertyChanged(nameof(AppliesToPregnant));
        OnPropertyChanged(nameof(LowNormal));
        OnPropertyChanged(nameof(HighNormal));
        OnPropertyChanged(nameof(LowCritical));
        OnPropertyChanged(nameof(HighCritical));
        OnPropertyChanged(nameof(NormalRangeText));
        OnPropertyChanged(nameof(RangeNote));
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
            AgeToDays = 36500
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
            AppliesToPregnant = range.AppliesToPregnant,
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
