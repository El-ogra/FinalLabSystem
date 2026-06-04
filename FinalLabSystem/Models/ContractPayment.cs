using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Contract Payment - Payment records for contract invoices
/// V4.0 New Table
/// </summary>
public partial class ContractPayment
{
    public int ContractPaymentId { get; set; }

    public int ContractInvoiceId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? ReferenceNumber { get; set; }

    // Navigation properties
    public virtual ContractInvoice ContractInvoice { get; set; } = null!;
}
