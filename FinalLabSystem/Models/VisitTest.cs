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

    /// <remarks>
    /// DEPRECATED: Use ExternalShipmentItem for outsourcing.
    /// These fields are kept for backward compatibility only.
    /// Do not populate them in new code.
    /// </remarks>
    public bool IsOutsourced { get; set; }

    /// <remarks>
    /// DEPRECATED: Use ExternalShipmentItem for outsourcing.
    /// These fields are kept for backward compatibility only.
    /// Do not populate them in new code.
    /// </remarks>
    public int? ExternalLabId { get; set; }

    /// <remarks>
    /// DEPRECATED: Use ExternalShipmentItem for outsourcing.
    /// These fields are kept for backward compatibility only.
    /// Do not populate them in new code.
    /// </remarks>
    public decimal? OutsourceCost { get; set; }

    /// <remarks>
    /// DEPRECATED: Use ExternalShipmentItem for outsourcing.
    /// These fields are kept for backward compatibility only.
    /// Do not populate them in new code.
    /// </remarks>
    public DateTime? OutsourceSentAt { get; set; }

    /// <remarks>
    /// DEPRECATED: Use ExternalShipmentItem for outsourcing.
    /// These fields are kept for backward compatibility only.
    /// Do not populate them in new code.
    /// </remarks>
    public int? OutsourceSentBy { get; set; }

    /// <remarks>
    /// DEPRECATED: Use ExternalShipmentItem for outsourcing.
    /// These fields are kept for backward compatibility only.
    /// Do not populate them in new code.
    /// </remarks>
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
