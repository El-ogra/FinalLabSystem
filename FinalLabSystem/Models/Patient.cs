using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string PatientCode { get; set; } = null!;

    public string? NationalId { get; set; }

    public string? Title { get; set; }

    public string FullNameAr { get; set; } = null!;

    public string? FullNameEn { get; set; }

    public string Sex { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public double? ApproxAge { get; set; }

    public string? ApproxAgeUnit { get; set; }

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? BloodType { get; set; }

    public string? Notes { get; set; }

    public bool IsVip { get; set; }

    public string PatientType { get; set; } = "Individual";

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    public virtual ICollection<PatientMedicalHistory> PatientMedicalHistories { get; set; } = new List<PatientMedicalHistory>();
}
