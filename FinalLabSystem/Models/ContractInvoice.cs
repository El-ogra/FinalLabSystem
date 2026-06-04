using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Contract Invoice - Monthly aggregated invoices to corporate clients
/// V4.0 New Table
/// </summary>
public partial class ContractInvoice
{
    public int ContractInvoiceId { get; set; }

    public int CompanyId { get; set; }

    public DateTime InvoiceDate { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; } = 0;

    public string Status { get; set; } = "DRAFT"; // DRAFT, ISSUED, PAID

    public int? CreatedBy { get; set; }

    // Navigation properties
    public virtual Company Company { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<ContractPayment> ContractPayments { get; set; } = new List<ContractPayment>();
}
