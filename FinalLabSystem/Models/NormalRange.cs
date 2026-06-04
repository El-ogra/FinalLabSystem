using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class NormalRange
{
    public int RangeId { get; set; }

    public int ComponentId { get; set; }

    public string Sex { get; set; } = null!;

    public int AgeFromDays { get; set; }

    public int AgeToDays { get; set; }

    public string? AgeDescription { get; set; }

    public bool? AppliesToPregnant { get; set; }

    public string FastingState { get; set; } = null!;

    public double? LowNormal { get; set; }

    public double? HighNormal { get; set; }

    public double? LowCritical { get; set; }

    public double? HighCritical { get; set; }

    public string? NormalRangeText { get; set; }

    public string? RangeNote { get; set; }

    public virtual TestComponent Component { get; set; } = null!;
}
