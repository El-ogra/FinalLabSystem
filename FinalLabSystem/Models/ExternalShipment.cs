using System;
using System.Collections.Generic;
using FinalLabSystem.Data;

namespace FinalLabSystem.Models;

/// <summary>
/// External Shipment - Manifests for samples sent to external reference labs
/// V4.0 New Table
/// </summary>
[Auditable]
public partial class ExternalShipment
{
    public int ShipmentId { get; set; }

    public int ExternalLabId { get; set; }

    public DateTime ShipmentDate { get; set; }

    public string Status { get; set; } = "PENDING"; // PENDING, SENT, RECEIVED, COMPLETED

    public int? CreatedBy { get; set; }

    public string? TrackingNumber { get; set; }

    // Navigation properties
    public virtual ExternalLab ExternalLab { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<ExternalShipmentItem> ExternalShipmentItems { get; set; } = new List<ExternalShipmentItem>();
}
