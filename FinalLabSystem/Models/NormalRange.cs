using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <remarks>
/// NormalRange rows are treated as immutable once saved.
/// To update a range: set IsActive = false on the old row,
/// set SupersededById on the old row pointing to the new one,
/// and insert a new row with Version = oldVersion + 1.
/// Do NOT update existing rows in place.
/// </remarks>
public partial class NormalRange
{
    public int RangeId { get; set; }

    public int ComponentId { get; set; }

    public string Sex { get; set; } = null!;

    public int AgeFromDays { get; set; }

    public int AgeToDays { get; set; }

    public int? AgeFromValue { get; set; }

    public int? AgeToValue { get; set; }

    public string? AgeDescription { get; set; }

    public bool? ForPregnantOnly { get; set; }

    public string? AgeUnit { get; set; }

    public string? LowFlag { get; set; }

    public string? HighFlag { get; set; }

    public string? LowComment { get; set; }

    public string? HighComment { get; set; }

    public string? CriticalRangeText { get; set; }

    public string? CriticalFlag { get; set; }

    public string? CriticalComment { get; set; }

    public string FastingState { get; set; } = null!;

    public double? LowNormal { get; set; }

    public double? HighNormal { get; set; }

    public double? LowCritical { get; set; }

    public double? HighCritical { get; set; }

    public string? NormalRangeText { get; set; }

    public string? RangeNote { get; set; }

    public string? Unit { get; set; }

    public int Version { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public int? SupersededById { get; set; }

    public NormalRange? SupersededBy { get; set; }

    public virtual TestComponent Component { get; set; } = null!;
}
