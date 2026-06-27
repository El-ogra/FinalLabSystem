using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAttendanceService
{
    /// <summary>
    /// Gets active work shifts.
    /// </summary>
    /// <returns>The active work shifts.</returns>
    Task<List<WorkShift>> GetActiveShiftsAsync();

    /// <summary>
    /// Records a staff clock-in event.
    /// </summary>
    /// <param name="staffId">The staff member identifier.</param>
    /// <param name="shiftId">The work shift identifier.</param>
    Task RecordClockInAsync(int staffId, int shiftId);

    /// <summary>
    /// Records a staff clock-out event.
    /// </summary>
    /// <param name="staffId">The staff member identifier.</param>
    Task RecordClockOutAsync(int staffId);

    /// <summary>
    /// Gets attendance records within a date range, optionally filtered by staff.
    /// </summary>
    Task<List<Attendance>> GetAttendanceByDateRangeAsync(DateOnly from, DateOnly to, int? staffId = null);

    /// <summary>
    /// Calculates total hours worked by a staff member in a date range.
    /// ClockOut=null records are excluded from the calculation.
    /// </summary>
    Task<TimeSpan> GetTotalHoursWorkedAsync(int staffId, DateOnly from, DateOnly to);

    /// <summary>
    /// Gets the active (un-clocked-out) attendance record for a staff member, or null.
    /// </summary>
    Task<Attendance?> GetActiveAttendanceAsync(int staffId);

    /// <summary>
    /// Gets all work shifts (including inactive).
    /// </summary>
    Task<List<WorkShift>> GetAllShiftsAsync();

    /// <summary>
    /// Creates a new work shift.
    /// </summary>
    Task<WorkShift> CreateShiftAsync(WorkShift shift);

    /// <summary>
    /// Updates an existing work shift.
    /// </summary>
    Task UpdateShiftAsync(WorkShift shift);

    /// <summary>
    /// Gets all active staff members.
    /// </summary>
    Task<List<Staff>> GetAllActiveStaffAsync();
}
