using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class AttendanceWindowViewModel : ViewModelBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IDialogService _dialogService;

    private AttendanceRowViewModel? _selectedAttendance;
    private WorkShiftRowViewModel? _selectedShift;
    private Staff? _selectedStaff;
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Today);
    private string _statusMessage = string.Empty;

    public AttendanceWindowViewModel(IAttendanceService attendanceService, IDialogService dialogService)
    {
        _attendanceService = attendanceService;
        _dialogService = dialogService;

        Attendances = new ObservableCollection<AttendanceRowViewModel>();
        WorkShifts = new ObservableCollection<WorkShiftRowViewModel>();
        StaffList = new ObservableCollection<Staff>();

        ClockInCommand = new AsyncRelayCommand(ExecuteClockInAsync, () => SelectedStaff is not null && SelectedShift is not null);
        ClockOutCommand = new AsyncRelayCommand(ExecuteClockOutAsync, () => SelectedStaff is not null);
        RefreshCommand = new AsyncRelayCommand(ExecuteRefreshAsync);
        SaveShiftCommand = new AsyncRelayCommand(ExecuteSaveShiftAsync);
    }

    public ObservableCollection<AttendanceRowViewModel> Attendances { get; }
    public ObservableCollection<WorkShiftRowViewModel> WorkShifts { get; }
    public ObservableCollection<Staff> StaffList { get; }

    public AttendanceRowViewModel? SelectedAttendance
    {
        get => _selectedAttendance;
        set => SetProperty(ref _selectedAttendance, value);
    }

    public WorkShiftRowViewModel? SelectedShift
    {
        get => _selectedShift;
        set => SetProperty(ref _selectedShift, value);
    }

    public Staff? SelectedStaff
    {
        get => _selectedStaff;
        set => SetProperty(ref _selectedStaff, value);
    }

    public DateOnly SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
                _ = LoadAttendanceAsync();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand ClockInCommand { get; }
    public ICommand ClockOutCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SaveShiftCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            var staff = await _attendanceService.GetAllActiveStaffAsync();
            StaffList.Clear();
            foreach (var s in staff)
                StaffList.Add(s);

            var shifts = await _attendanceService.GetAllShiftsAsync();
            WorkShifts.Clear();
            foreach (var sh in shifts)
                WorkShifts.Add(new WorkShiftRowViewModel
                {
                    ShiftId = sh.ShiftId,
                    ShiftName = sh.ShiftName,
                    ClockInTime = sh.ClockInTime,
                    ClockOutTime = sh.ClockOutTime,
                    IsActive = sh.IsActive
                });

            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تحميل البيانات: {ex.Message}");
        }
    }

    private async Task LoadAttendanceAsync()
    {
        try
        {
            var records = await _attendanceService.GetAttendanceByDateRangeAsync(SelectedDate, SelectedDate);
            Attendances.Clear();
            foreach (var r in records)
            {
                var vm = new AttendanceRowViewModel();
                vm.LoadFromModel(r, r.Staff.DisplayName, r.Shift?.ShiftName);
                Attendances.Add(vm);
            }
            StatusMessage = $"تم تحميل {Attendances.Count} سجل حضور";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تحميل سجلات الحضور: {ex.Message}");
        }
    }

    private async Task ExecuteClockInAsync()
    {
        if (SelectedStaff is null || SelectedShift is null) return;

        try
        {
            await _attendanceService.RecordClockInAsync(SelectedStaff.StaffId, SelectedShift.ShiftId);
            _dialogService.ShowMessage($"تم تسجيل دخول {SelectedStaff.DisplayName} بنجاح", "تسجيل الحضور");
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تسجيل الدخول: {ex.Message}");
        }
    }

    private async Task ExecuteClockOutAsync()
    {
        if (SelectedStaff is null) return;

        try
        {
            await _attendanceService.RecordClockOutAsync(SelectedStaff.StaffId);
            _dialogService.ShowMessage($"تم تسجيل خروج {SelectedStaff.DisplayName} بنجاح", "تسجيل الحضور");
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في تسجيل الخروج: {ex.Message}");
        }
    }

    private async Task ExecuteRefreshAsync()
    {
        await LoadAttendanceAsync();
    }

    private async Task ExecuteSaveShiftAsync()
    {
        try
        {
            var newShift = new WorkShift
            {
                ShiftName = "وردية جديدة",
                ClockInTime = new TimeSpan(8, 0, 0),
                ClockOutTime = new TimeSpan(16, 0, 0),
                IsActive = true
            };
            await _attendanceService.CreateShiftAsync(newShift);
            await LoadAsync();
            _dialogService.ShowMessage("تم إنشاء وردية جديدة", "إدارة الورديات");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"خطأ في إنشاء الوردية: {ex.Message}");
        }
    }
}
