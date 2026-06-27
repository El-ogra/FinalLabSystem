using System;

namespace FinalLabSystem.Models.DTOs;

public class CommissionReportRow
{
    public int ReferralId { get; set; }
    public string? ReferralName { get; set; }
    public string SourceType { get; set; } = null!;
    public double CommissionRate { get; set; }
    public int VisitId { get; set; }
    public string VisitCode { get; set; } = null!;
    public DateTime VisitDate { get; set; }
    public string PatientName { get; set; } = null!;
    public double VisitTotal { get; set; }
    public double TotalPaid { get; set; }
    public double? CommissionDue { get; set; }
}
