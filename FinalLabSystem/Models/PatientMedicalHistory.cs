using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Patient Medical History - Structured medical history record (diseases, allergies, medications, surgeries)
/// V4.0 New Table
/// </summary>
public partial class PatientMedicalHistory
{
    public int MedicalHistoryId { get; set; }

    public int PatientId { get; set; }

    public string HistoryType { get; set; } = null!; // DISEASE, MEDICATION, ALLERGY, SURGERY, OTHER

    public string Description { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }
}
