using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class CrossMatchDonor
{
    public int DonorResultId { get; set; }

    public int CrossmatchId { get; set; }

    public string DonorUnitNumber { get; set; } = null!;

    public string? DonorBloodType { get; set; }

    public string? DonorRhFactor { get; set; }

    public string? MajorCrossmatch { get; set; }

    public string? MinorCrossmatch { get; set; }

    public string? DirectAntiglobulin { get; set; }

    public string? IndirectAntiglobulin { get; set; }

    public string UnitResult { get; set; } = null!;

    public string? ResultNote { get; set; }

    public virtual CrossMatchTest Crossmatch { get; set; } = null!;
}
