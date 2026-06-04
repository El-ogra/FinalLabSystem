using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class CrossMatchTest
{
    public int CrossmatchId { get; set; }

    public int VisitTestId { get; set; }

    public string? RecipientBloodType { get; set; }

    public string? RecipientRhFactor { get; set; }

    public string? RecipientAntibodyScreen { get; set; }

    public string OverallResult { get; set; } = null!;

    public DateTime? TestedAt { get; set; }

    public int? TestedBy { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<CrossMatchDonor> CrossMatchDonors { get; set; } = new List<CrossMatchDonor>();

    public virtual Staff? TestedByNavigation { get; set; }

    public virtual VisitTest VisitTest { get; set; } = null!;
}
