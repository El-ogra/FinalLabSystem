using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Antibiotic Catalog - Centralized registry of antibiotics with safety flags for pregnancy and children
/// V4.0 New Table
/// </summary>
public partial class AntibioticCatalog
{
    public int AntibioticId { get; set; }

    public string AntibioticName { get; set; } = null!;

    public string? AntibioticClass { get; set; }

    public bool IsSafePregnancy { get; set; } = false;

    public bool IsSafeChildren { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<OrganismAntibiotic> OrganismAntibiotics { get; set; } = new List<OrganismAntibiotic>();
}
