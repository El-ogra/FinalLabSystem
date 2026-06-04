using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class ReferralSource
{
    public int ReferralId { get; set; }

    public string SourceType { get; set; } = null!;

    public string? Title { get; set; }

    public string SourceName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public double CommissionRate { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? SchemeId { get; set; }

    // Navigation properties
    public virtual PriceScheme? Scheme { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
