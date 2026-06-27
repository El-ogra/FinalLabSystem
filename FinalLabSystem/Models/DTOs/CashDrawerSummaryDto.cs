using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models.DTOs;

public sealed class CashDrawerSummaryDto
{
    public DateOnly Date { get; set; }
    public decimal TotalCashReceived { get; set; }
    public decimal TotalInsuranceReceived { get; set; }
    public decimal TotalContractReceived { get; set; }
    public decimal GrandTotal { get; set; }
    public int PaymentCount { get; set; }
    public List<CashDrawerPaymentRow> Payments { get; set; } = new();
}

public sealed class CashDrawerPaymentRow
{
    public int PaymentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string VisitCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
}
