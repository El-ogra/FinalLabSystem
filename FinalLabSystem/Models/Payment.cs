using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int VisitId { get; set; }

    public DateTime PaymentDate { get; set; }

    public double Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentType { get; set; } = null!;

    public int ReceivedBy { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public virtual Staff ReceivedByNavigation { get; set; } = null!;

    public virtual Visit Visit { get; set; } = null!;
}
