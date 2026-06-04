using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Attendance - Employee clock-in/clock-out records with tardiness tracking
/// V4.0 New Table
/// </summary>
public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int StaffId { get; set; }

    public int? ShiftId { get; set; }

    public DateTime ClockIn { get; set; }

    public DateTime? ClockOut { get; set; }

    public int? LateMinutes { get; set; }

    public string? AbsenceStatus { get; set; }

    public DateOnly AttendanceDate { get; set; }

    // Navigation properties
    public virtual Staff Staff { get; set; } = null!;

    public virtual WorkShift? Shift { get; set; }
}
