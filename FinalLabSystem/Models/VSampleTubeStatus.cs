using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class VSampleTubeStatus
{
    public int TubeId { get; set; }

    public int VisitId { get; set; }

    public string VisitCode { get; set; } = null!;

    public string PatientName { get; set; } = null!;

    public string TubeType { get; set; } = null!;

    public string? TubeColor { get; set; }

    public string BarcodeValue { get; set; } = null!;

    public DateTime? CollectedAt { get; set; }

    public string? CollectedByName { get; set; }

    public DateTime? PrintedAt { get; set; }

    public string? PrintedByName { get; set; }

    public int? TestsOnThisTube { get; set; }
}
