using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models.DTOs;

public sealed class TodayPatientWithStatusDto
{
    public int PatientId { get; set; }

    public int VisitId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public string FullNameAr { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Sex { get; set; }

    public int? ApproxAge { get; set; }

    public string? ApproxAgeUnit { get; set; }

    public bool IsVip { get; set; }

    public string? ReferralName { get; set; }

    public int VisitCount { get; set; }

    public PatientVisitStatus ComputedStatus { get; set; }

    public string StatusIcon { get; set; } = string.Empty;

    public string StatusColor { get; set; } = string.Empty;

    public decimal BalanceDue { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string? VisitNotes { get; set; }

    public string? VisitCode { get; set; }

    public string PatientType { get; set; } = "Individual";
}
