using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Test Profile/Package - Groups of predefined tests that can be added to a visit with a single click
/// V4.0 New Table
/// </summary>
public partial class TestProfile
{
    public int ProfileId { get; set; }

    public string ProfileNameAr { get; set; } = null!;

    public string? ProfileNameEn { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    // Navigation properties
    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<TestProfileItem> TestProfileItems { get; set; } = new List<TestProfileItem>();
}
