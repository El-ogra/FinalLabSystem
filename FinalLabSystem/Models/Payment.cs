using System;
using System.Collections.Generic;
using FinalLabSystem.Data;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models;

[Auditable]

public partial class Payment
{
    public int PaymentId { get; set; }

    public int VisitId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string PaymentType { get; set; } = null!;

    public int ReceivedBy { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public virtual Staff ReceivedByNavigation { get; set; } = null!;

    public virtual Visit Visit { get; set; } = null!;
}
