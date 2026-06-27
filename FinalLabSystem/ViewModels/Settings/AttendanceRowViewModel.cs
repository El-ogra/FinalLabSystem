using System;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class AttendanceRowViewModel : ViewModelBase
{
    private int _attendanceId;
    private int _staffId;
    private string _staffName = string.Empty;
    private int? _shiftId;
    private string _shiftName = string.Empty;
    private DateTime _clockIn;
    private DateTime? _clockOut;
    private int? _lateMinutes;
    private DateOnly _attendanceDate;

    public int AttendanceId
    {
        get => _attendanceId;
        set => SetProperty(ref _attendanceId, value);
    }

    public int StaffId
    {
        get => _staffId;
        set => SetProperty(ref _staffId, value);
    }

    public string StaffName
    {
        get => _staffName;
        set => SetProperty(ref _staffName, value);
    }

    public int? ShiftId
    {
        get => _shiftId;
        set => SetProperty(ref _shiftId, value);
    }

    public string ShiftName
    {
        get => _shiftName;
        set => SetProperty(ref _shiftName, value);
    }

    public DateTime ClockIn
    {
        get => _clockIn;
        set
        {
            if (SetProperty(ref _clockIn, value))
                OnPropertyChanged(nameof(ClockInTime));
        }
    }

    public DateTime? ClockOut
    {
        get => _clockOut;
        set
        {
            if (SetProperty(ref _clockOut, value))
            {
                OnPropertyChanged(nameof(ClockOutTime));
                OnPropertyChanged(nameof(IsClockedIn));
                OnPropertyChanged(nameof(HoursWorkedDisplay));
            }
        }
    }

    public int? LateMinutes
    {
        get => _lateMinutes;
        set => SetProperty(ref _lateMinutes, value);
    }

    public DateOnly AttendanceDate
    {
        get => _attendanceDate;
        set => SetProperty(ref _attendanceDate, value);
    }

    public string ClockInTime => ClockIn.ToString("HH:mm");
    public string ClockOutTime => ClockOut?.ToString("HH:mm") ?? "—";
    public bool IsClockedIn => ClockOut is null;

    public string HoursWorkedDisplay
    {
        get
        {
            if (ClockOut is null) return "—";
            var span = ClockOut.Value - ClockIn;
            return $"{(int)span.TotalHours}:{span.Minutes:D2}";
        }
    }

    public void LoadFromModel(Attendance attendance, string staffName, string? shiftName)
    {
        AttendanceId = attendance.AttendanceId;
        StaffId = attendance.StaffId;
        StaffName = staffName;
        ShiftId = attendance.ShiftId;
        ShiftName = shiftName ?? "—";
        ClockIn = attendance.ClockIn;
        ClockOut = attendance.ClockOut;
        LateMinutes = attendance.LateMinutes;
        AttendanceDate = attendance.AttendanceDate;
    }
}
