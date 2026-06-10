using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinalLabSystem.Data;

namespace FinalLabSystem.Models;

[Auditable]

public partial class Patient
{
    public int PatientId { get; set; }

    [Required(ErrorMessage = "كود المريض مطلوب")]
    [StringLength(30)]
    public string PatientCode { get; set; } = null!;

    [StringLength(20)]
    public string? NationalId { get; set; }

    [StringLength(50)]
    public string? Title { get; set; }

    [Required(ErrorMessage = "اسم المريض بالعربية مطلوب")]
    [StringLength(200)]
    public string FullNameAr { get; set; } = null!;

    [StringLength(200)]
    public string? FullNameEn { get; set; }

    [Required(ErrorMessage = "الجنس مطلوب")]
    [StringLength(1)]
    public string Sex { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public int? ApproxAge { get; set; }

    public string? ApproxAgeUnit { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(20)]
    public string? Phone2 { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(5)]
    public string? BloodType { get; set; }

    public string? Notes { get; set; }

    public bool IsVip { get; set; }

    [StringLength(20)]
    public string PatientType { get; set; } = "Individual";

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    public virtual ICollection<PatientMedicalHistory> PatientMedicalHistories { get; set; } = new List<PatientMedicalHistory>();
}
