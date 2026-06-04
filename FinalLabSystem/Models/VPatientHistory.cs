using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class VPatientHistory
{
    public int PatientId { get; set; }

    public string PatientCode { get; set; } = null!;

    public string FullNameAr { get; set; } = null!;

    public string Sex { get; set; } = null!;

    public int VisitId { get; set; }

    public string VisitCode { get; set; } = null!;

    public DateTime VisitDate { get; set; }

    public bool IsFasting { get; set; }

    public string? TestCategory { get; set; }

    public string? TestGroup { get; set; }

    public string? TestType { get; set; }

    public string? ComponentName { get; set; }

    public string? ResultValue { get; set; }

    public string? Unit { get; set; }

    public string? ResultStatus { get; set; }

    public double? SnapLowNormal { get; set; }

    public double? SnapHighNormal { get; set; }

    public string? NormalRange { get; set; }

    public DateTime? EnteredAt { get; set; }
}
