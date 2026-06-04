using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestWorkflow
{
    public int WorkflowId { get; set; }

    public int VisitTestId { get; set; }

    public string Stage { get; set; } = null!;

    public int PerformedBy { get; set; }

    public DateTime PerformedAt { get; set; }

    public string? Notes { get; set; }

    public virtual Staff PerformedByNavigation { get; set; } = null!;

    public virtual VisitTest VisitTest { get; set; } = null!;
}
