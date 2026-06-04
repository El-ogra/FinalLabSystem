using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class VOutstandingBalance
{
    public int VisitId { get; set; }

    public string VisitCode { get; set; } = null!;

    public DateTime VisitDate { get; set; }

    public string PatientCode { get; set; } = null!;

    public string PatientName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? CompanyName { get; set; }

    public double TotalAfterDiscount { get; set; }

    public double TotalPaid { get; set; }

    public double BalanceDue { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public int? DaysOverdue { get; set; }
}
