using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Visit
{
    public int VisitId { get; set; }

    public string VisitCode { get; set; } = null!;

    public int PatientId { get; set; }

    public DateTime VisitDate { get; set; }

    public DateTime? ExpectedReady { get; set; }

    public bool IsPregnant { get; set; }

    public bool IsFasting { get; set; }

    public short? FastingHours { get; set; }

    public int? ReferralId { get; set; }

    public int? CompanyId { get; set; }

    public int? SchemeId { get; set; }

    public double Subtotal { get; set; }

    public double DiscountAmount { get; set; }

    public double DiscountPercent { get; set; }

    public double TotalAfterDiscount { get; set; }

    public double TotalPaid { get; set; }

    public double BalanceDue { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string VisitStatus { get; set; } = null!;

    public int? ReceptionistId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Staff? Receptionist { get; set; }

    public virtual ReferralSource? Referral { get; set; }

    public virtual ICollection<SampleTube> SampleTubes { get; set; } = new List<SampleTube>();

    public virtual PriceScheme? Scheme { get; set; }

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();

    public virtual ICollection<VisitCharge> VisitCharges { get; set; } = new List<VisitCharge>();
}
