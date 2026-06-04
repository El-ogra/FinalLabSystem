using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class PriceScheme
{
    public int SchemeId { get; set; }

    public string SchemeName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public virtual ICollection<TestTypePrice> TestTypePrices { get; set; } = new List<TestTypePrice>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    public virtual ICollection<ReferralSource> ReferralSources { get; set; } = new List<ReferralSource>();
}
