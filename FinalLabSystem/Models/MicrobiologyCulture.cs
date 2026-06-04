using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class MicrobiologyCulture
{
    public int CultureId { get; set; }

    public int VisitTestId { get; set; }

    public string? SpecimenSource { get; set; }

    public double? SpecimenVolumeMl { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public int? InoculatedBy { get; set; }

    public string CultureResult { get; set; } = null!;

    public short? IncubationHours { get; set; }

    public DateTime? FinalReadingAt { get; set; }

    public int? ReadBy { get; set; }

    public string? FinalComment { get; set; }

    public virtual Staff? InoculatedByNavigation { get; set; }

    public virtual ICollection<MicrobiologyOrganism> MicrobiologyOrganisms { get; set; } = new List<MicrobiologyOrganism>();

    public virtual Staff? ReadByNavigation { get; set; }

    public virtual VisitTest VisitTest { get; set; } = null!;
}
