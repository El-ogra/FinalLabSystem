using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class OrganismAntibiotic
{
    public int AntibioticResultId { get; set; }

    public int OrganismId { get; set; }

    public string AntibioticName { get; set; } = null!;

    public string? AntibioticClass { get; set; }

    public string Sensitivity { get; set; } = null!;

    public string? MicValue { get; set; }

    public double? DiskDiffusionMm { get; set; }

    public string? BreakpointStandard { get; set; }

    public int? AntibioticCatalogId { get; set; }

    // Navigation properties
    public virtual MicrobiologyOrganism Organism { get; set; } = null!;

    public virtual AntibioticCatalog? Antibiotic { get; set; }
}
