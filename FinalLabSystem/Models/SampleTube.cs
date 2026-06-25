using System;
using System.Collections.Generic;
using FinalLabSystem.Data;

namespace FinalLabSystem.Models;

[Auditable]
public partial class SampleTube
{
    public int TubeId { get; set; }

    public int VisitId { get; set; }

    public string TubeType { get; set; } = null!;

    public string? TubeColor { get; set; }

    public string BarcodeValue { get; set; } = null!;

    public DateTime? CollectedAt { get; set; }

    public int? CollectedBy { get; set; }

    public DateTime? PrintedAt { get; set; }

    public int? PrintedBy { get; set; }

    public string? Notes { get; set; }

    public virtual Staff? CollectedByNavigation { get; set; }

    public virtual Staff? PrintedByNavigation { get; set; }

    public virtual Visit Visit { get; set; } = null!;

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();
}
