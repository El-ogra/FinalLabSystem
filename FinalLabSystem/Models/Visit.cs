using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FinalLabSystem.Data;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models;

[Auditable]

public partial class Visit
{
    public int VisitId { get; set; }

    [Required]
    [StringLength(30)]
    public string VisitCode { get; set; } = null!;

    public int PatientId { get; set; }

    public DateTime VisitDate { get; set; }

    public DateTime? ExpectedReady { get; set; }

    public bool IsPregnant { get; set; }

    public bool IsFasting { get; set; }

    public short? FastingHours { get; set; }

    public bool TakenOutsideLab { get; set; }

    public bool OutsideUrine { get; set; }

    public bool OutsideStool { get; set; }

    public bool OutsideBlood { get; set; }

    public bool OutsideSemen { get; set; }

    public bool OutsideCsf { get; set; }

    public bool HasDiabetes { get; set; }

    public bool HasAnemia { get; set; }

    public bool HasBleedingDisorder { get; set; }

    public bool HasThyroid { get; set; }

    public bool HasJointDisease { get; set; }

    public bool HasViralInfection { get; set; }

    public bool OnAnticoagulant { get; set; }

    public bool HasHypertension { get; set; }

    public bool HasLiverDisease { get; set; }

    public bool HasKidneyDisease { get; set; }

    public bool HasLupus { get; set; }

    public bool HadXrayContrast { get; set; }

    public int? ReferralId { get; set; }

    public int? CompanyId { get; set; }

    public int? SchemeId { get; set; }

    /// <summary>
    /// [القرار 12 - VS-01] نوع الفوترة على مستوى الزيارة.
    /// Individual = فرد | LabToLab = معمل-لمعمل | Free = مجاني.
    /// </summary>
    public BillingType BillingType { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal TotalAfterDiscount { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal BalanceDue { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public VisitStatus VisitStatus { get; set; }

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

    public DateTime? DeliveryConfirmedAt { get; set; }

    public byte[]? DeliverySignature { get; set; }

    [StringLength(256)]
    public string? DeliveryOtpCode { get; set; }

    public virtual ICollection<DeliveryConfirmation> DeliveryConfirmations { get; set; } = new List<DeliveryConfirmation>();

    // VS-03: 5 Boolean Flags
    public bool IsEntered { get; set; }
    public bool IsReviewed { get; set; }
    public bool IsPrinted { get; set; }
    public bool IsDelivered { get; set; }
    public bool IsFullyPaid { get; set; }

    [NotMapped]
    public VisitDisplayStatus VisitDisplayStatus
    {
        get
        {
            if (!IsEntered && !IsReviewed && !IsPrinted && !IsDelivered)
            {
                // S2: Partial Entry
                bool hasAnyResult = VisitTests != null && VisitTests
                    .SelectMany(vt => vt.TestResults ?? Enumerable.Empty<TestResult>())
                    .Any(r => r.ValidationStatus >= ResultValidationStatus.Entered);

                if (hasAnyResult)
                {
                    return VisitDisplayStatus.ResultsNotWritten; // S2 📝 (Partial)
                }

                // S1: New / No Results
                return VisitDisplayStatus.NewNoResults; // S1 🔴
            }

            if (IsEntered && !IsReviewed) return VisitDisplayStatus.ResultsNotReviewed; // S3 ↔️
            if (IsReviewed && !IsPrinted) return VisitDisplayStatus.ResultsNotPrinted; // S4 🖨️
            if (IsPrinted && !IsDelivered) return VisitDisplayStatus.NotDelivered; // S5 🛒
            if (IsDelivered && !IsFullyPaid) return VisitDisplayStatus.DeliveredWithBalance; // S6 £
            if (IsDelivered && IsFullyPaid) return VisitDisplayStatus.FullyComplete; // S7 🎖️

            return VisitDisplayStatus.NewNoResults; // fallback
        }
    }
}
