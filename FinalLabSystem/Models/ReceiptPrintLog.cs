using System;

namespace FinalLabSystem.Models;

public partial class ReceiptPrintLog
{
    public long LogId { get; set; }

    public int VisitId { get; set; }

    public int StaffId { get; set; }

    public DateTime PrintedAt { get; set; }

    /// <summary>
    /// "A4" or "Thermal"
    /// </summary>
    public string Format { get; set; } = null!;

    public bool ShowBreakdown { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAfterDiscount { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal BalanceDue { get; set; }

    public virtual Visit Visit { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
