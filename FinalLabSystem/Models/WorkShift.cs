using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Work Shift - Defines lab work shifts (Morning, Evening, Night)
/// V4.0 New Table
/// </summary>
public partial class WorkShift
{
    public int ShiftId { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeSpan ClockInTime { get; set; }

    public TimeSpan ClockOutTime { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
