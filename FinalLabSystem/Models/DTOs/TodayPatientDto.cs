namespace FinalLabSystem.Models.DTOs;

public sealed class TodayPatientDto
{
    public int PatientId { get; set; }

    public int VisitId { get; set; }

    public string PatientCode { get; set; } = string.Empty;

    public string FullNameAr { get; set; } = string.Empty;
}
