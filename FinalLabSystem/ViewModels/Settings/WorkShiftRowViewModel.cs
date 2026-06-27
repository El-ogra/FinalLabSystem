using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class WorkShiftRowViewModel : ViewModelBase
{
    private int _shiftId;
    private string _shiftName = string.Empty;
    private TimeSpan _clockInTime;
    private TimeSpan _clockOutTime;
    private bool _isActive = true;

    public int ShiftId
    {
        get => _shiftId;
        set => SetProperty(ref _shiftId, value);
    }

    public string ShiftName
    {
        get => _shiftName;
        set => SetProperty(ref _shiftName, value);
    }

    public TimeSpan ClockInTime
    {
        get => _clockInTime;
        set
        {
            if (SetProperty(ref _clockInTime, value))
                OnPropertyChanged(nameof(ClockInDisplay));
        }
    }

    public TimeSpan ClockOutTime
    {
        get => _clockOutTime;
        set
        {
            if (SetProperty(ref _clockOutTime, value))
                OnPropertyChanged(nameof(ClockOutDisplay));
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string ClockInDisplay => new DateTime(1, 1, 1, ClockInTime.Hours, ClockInTime.Minutes, 0).ToString("hh:mm tt");
    public string ClockOutDisplay => new DateTime(1, 1, 1, ClockOutTime.Hours, ClockOutTime.Minutes, 0).ToString("hh:mm tt");

    public void LoadFromModel(WorkShift shift)
    {
        ShiftId = shift.ShiftId;
        ShiftName = shift.ShiftName;
        ClockInTime = shift.ClockInTime;
        ClockOutTime = shift.ClockOutTime;
        IsActive = shift.IsActive;
    }

    public WorkShift ToModel() => new()
    {
        ShiftId = ShiftId,
        ShiftName = ShiftName,
        ClockInTime = ClockInTime,
        ClockOutTime = ClockOutTime,
        IsActive = IsActive
    };
}
