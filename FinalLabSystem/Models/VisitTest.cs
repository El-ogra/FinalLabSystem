using System;
using System.Collections.Generic;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models;

public partial class VisitTest
{
    public int VisitTestId { get; set; }

    public int VisitId { get; set; }

    public int TesttypeId { get; set; }

    public int? TubeId { get; set; }

    public decimal PriceCharged { get; set; }

    public TestStage CurrentStage { get; set; }

    public bool IsOutsourced { get; set; }

    public int? ExternalLabId { get; set; }

    public decimal? OutsourceCost { get; set; }

    public DateTime? OutsourceSentAt { get; set; }

    public int? OutsourceSentBy { get; set; }

    public DateTime? OutsourceResultReceivedAt { get; set; }

    public DateTime AddedAt { get; set; }

    public int? AddedBy { get; set; }

    public virtual Staff? AddedByNavigation { get; set; }

    public virtual CrossMatchTest? CrossMatchTest { get; set; }

    public virtual ExternalLab? ExternalLab { get; set; }

    public virtual MicrobiologyCulture? MicrobiologyCulture { get; set; }

    public virtual Staff? OutsourceSentByNavigation { get; set; }

    public virtual SemenAnalysis? SemenAnalysis { get; set; }

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

    public virtual ICollection<TestWorkflow> TestWorkflows { get; set; } = new List<TestWorkflow>();

    public virtual TestType Testtype { get; set; } = null!;

    public virtual SampleTube? Tube { get; set; }

    public virtual Visit Visit { get; set; } = null!;

    public virtual ICollection<ExternalShipmentItem> ExternalShipmentItems { get; set; } = new List<ExternalShipmentItem>();
}
