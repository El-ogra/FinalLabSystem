namespace FinalLabSystem.Models.DTOs;

public sealed class VisitFullDto
{
    public int PatientId { get; set; }

    public int VisitId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public string FullNameAr { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string Sex { get; set; } = "U";

    public string PatientType { get; set; } = "Individual";

    public bool IsVip { get; set; }

    public int? ApproxAge { get; set; }

    public string ApproxAgeUnit { get; set; } = "Years";

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? NationalId { get; set; }

    public string? Notes { get; set; }

    public DateTime EntryDate { get; set; }

    public DateTime? ExpectedReady { get; set; }

    public int? ReferralId { get; set; }

    public string? ReferralTitle { get; set; }

    public string? ReferralName { get; set; }

    public string? ReferralAddress { get; set; }

    public bool IsFasting { get; set; }

    public short? FastingHours { get; set; }

    public bool IsPregnant { get; set; }

    public string? VisitNotes { get; set; }

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

    public List<SelectedTestDto> SelectedTests { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal TotalAfterDiscount { get; set; }

    public decimal TotalPaid { get; set; }

    public decimal BalanceDue { get; set; }

    public string PaymentStatus { get; set; } = "PENDING";
}
