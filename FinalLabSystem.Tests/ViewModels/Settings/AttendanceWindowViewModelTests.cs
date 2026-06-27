using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class AttendanceWindowViewModelTests
{
    private static (AttendanceWindowViewModel VM, Mock<IAttendanceService> AttendanceMock, Mock<IDialogService> DialogMock) CreateVM()
    {
        var attendanceMock = new Mock<IAttendanceService>();
        var dialogMock = new Mock<IDialogService>();
        var vm = new AttendanceWindowViewModel(attendanceMock.Object, dialogMock.Object);
        return (vm, attendanceMock, dialogMock);
    }

    [Fact]
    public async Task LoadAsync_PopulatesStaffAndShifts()
    {
        var (vm, attendanceMock, _) = CreateVM();
        attendanceMock.Setup(a => a.GetAllActiveStaffAsync())
            .ReturnsAsync(new List<Staff>
            {
                new() { StaffId = 1, DisplayName = "User A", IsActive = true },
                new() { StaffId = 2, DisplayName = "User B", IsActive = true }
            });
        attendanceMock.Setup(a => a.GetAllShiftsAsync())
            .ReturnsAsync(new List<WorkShift>
            {
                new() { ShiftId = 1, ShiftName = "Morning", ClockInTime = TimeSpan.FromHours(8), ClockOutTime = TimeSpan.FromHours(16), IsActive = true }
            });
        attendanceMock.Setup(a => a.GetAttendanceByDateRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Attendance>());

        await vm.LoadAsync();

        Assert.Equal(2, vm.StaffList.Count);
        Assert.Single(vm.WorkShifts);
    }

    [Fact]
    public async Task ClockInAsync_WhenStaffAndShiftSelected_CallsService()
    {
        var (vm, attendanceMock, dialogMock) = CreateVM();
        attendanceMock.Setup(a => a.GetAllActiveStaffAsync())
            .ReturnsAsync(new List<Staff> { new() { StaffId = 1, DisplayName = "A", IsActive = true } });
        attendanceMock.Setup(a => a.GetAllShiftsAsync())
            .ReturnsAsync(new List<WorkShift> { new() { ShiftId = 1, ShiftName = "M", ClockInTime = TimeSpan.FromHours(8), ClockOutTime = TimeSpan.FromHours(16), IsActive = true } });
        attendanceMock.Setup(a => a.GetAttendanceByDateRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Attendance>());

        await vm.LoadAsync();
        vm.SelectedStaff = vm.StaffList[0];
        vm.SelectedShift = vm.WorkShifts[0];

        vm.ClockInCommand.Execute(null);

        attendanceMock.Verify(a => a.RecordClockInAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task ClockOutAsync_WhenStaffSelected_CallsService()
    {
        var (vm, attendanceMock, _) = CreateVM();
        attendanceMock.Setup(a => a.GetAllActiveStaffAsync())
            .ReturnsAsync(new List<Staff> { new() { StaffId = 1, DisplayName = "A", IsActive = true } });
        attendanceMock.Setup(a => a.GetAllShiftsAsync())
            .ReturnsAsync(new List<WorkShift>());
        attendanceMock.Setup(a => a.GetAttendanceByDateRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Attendance>());

        await vm.LoadAsync();
        vm.SelectedStaff = vm.StaffList[0];

        vm.ClockOutCommand.Execute(null);

        attendanceMock.Verify(a => a.RecordClockOutAsync(1), Times.Once);
    }

    [Fact]
    public async Task ClockInAsync_WhenServiceThrows_ShowsErrorDialog()
    {
        var (vm, attendanceMock, dialogMock) = CreateVM();
        attendanceMock.Setup(a => a.GetAllActiveStaffAsync())
            .ReturnsAsync(new List<Staff> { new() { StaffId = 1, DisplayName = "A", IsActive = true } });
        attendanceMock.Setup(a => a.GetAllShiftsAsync())
            .ReturnsAsync(new List<WorkShift> { new() { ShiftId = 1, ShiftName = "M", ClockInTime = TimeSpan.FromHours(8), ClockOutTime = TimeSpan.FromHours(16), IsActive = true } });
        attendanceMock.Setup(a => a.GetAttendanceByDateRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(new List<Attendance>());
        attendanceMock.Setup(a => a.RecordClockInAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("DB error"));

        await vm.LoadAsync();
        vm.SelectedStaff = vm.StaffList[0];
        vm.SelectedShift = vm.WorkShifts[0];

        vm.ClockInCommand.Execute(null);

        dialogMock.Verify(d => d.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ClockInCommand_CanExecute_FalseWhenNoStaff()
    {
        var (vm, _, _) = CreateVM();

        Assert.False(vm.ClockInCommand.CanExecute(null));
    }

    [Fact]
    public void ClockOutCommand_CanExecute_FalseWhenNoStaff()
    {
        var (vm, _, _) = CreateVM();

        Assert.False(vm.ClockOutCommand.CanExecute(null));
    }
}
