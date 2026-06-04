using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class MicrobiologyOrganism
{
    public int OrganismId { get; set; }

    public int CultureId { get; set; }

    public string OrganismName { get; set; } = null!;

    public string? ColonyCount { get; set; }

    public string? GramStain { get; set; }

    public string? Morphology { get; set; }

    public byte SortOrder { get; set; }

    public virtual MicrobiologyCulture Culture { get; set; } = null!;

    public virtual ICollection<OrganismAntibiotic> OrganismAntibiotics { get; set; } = new List<OrganismAntibiotic>();
}
