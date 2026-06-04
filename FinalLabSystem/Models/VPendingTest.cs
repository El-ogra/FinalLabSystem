using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class VPendingTest
{
    public int VisitId { get; set; }

    public string VisitCode { get; set; } = null!;

    public DateTime VisitDate { get; set; }

    public string PatientName { get; set; } = null!;

    public string? TestName { get; set; }

    public string? SpecialType { get; set; }

    public int VisitTestId { get; set; }

    public string CurrentStage { get; set; } = null!;

    public bool IsOutsourced { get; set; }

    public string? ExternalLab { get; set; }

    public DateTime? LastStageAt { get; set; }

    public string? LastStageBy { get; set; }
}
