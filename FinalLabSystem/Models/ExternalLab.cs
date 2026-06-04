using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class ExternalLab
{
    public int ExternalLabId { get; set; }

    public string LabName { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();

    public virtual ICollection<ExternalShipment> ExternalShipments { get; set; } = new List<ExternalShipment>();
}
