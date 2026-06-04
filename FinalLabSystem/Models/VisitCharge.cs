using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Visit Charge - Additional charges beyond tests (e.g., home collection, rush fee)
/// V4.0 New Table
/// </summary>
public partial class VisitCharge
{
    public int ChargeId { get; set; }

    public int VisitId { get; set; }

    public string ChargeDescription { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? ChargeType { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Visit Visit { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }
}
