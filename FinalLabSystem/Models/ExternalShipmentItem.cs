using System;
using System.Collections.Generic;
using FinalLabSystem.Data;

namespace FinalLabSystem.Models;

/// <summary>
/// External Shipment Item - Line items detailing individual samples in each shipment
/// V4.0 New Table
/// </summary>
[Auditable]
public partial class ExternalShipmentItem
{
    public int ShipmentItemId { get; set; }

    public int ShipmentId { get; set; }

    public int VisitTestId { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    // Navigation properties
    public virtual ExternalShipment Shipment { get; set; } = null!;

    public virtual VisitTest VisitTest { get; set; } = null!;
}
