using System;
using System.Collections.Generic;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Models;

public partial class TestResult
{
    public int ResultId { get; set; }

    public int VisitTestId { get; set; }

    public int ComponentId { get; set; }

    public string? ResultValue { get; set; }

    public decimal? ResultNumeric { get; set; }

    public string? ResultStatus { get; set; }

    public string? SnapUnit { get; set; }

    public double? SnapLowNormal { get; set; }

    public double? SnapHighNormal { get; set; }

    public double? SnapLowCritical { get; set; }

    public double? SnapHighCritical { get; set; }

    public string? SnapNormalText { get; set; }

    public int? EnteredBy { get; set; }

    public DateTime? EnteredAt { get; set; }

    public int? LastModifiedBy { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public string? Comment { get; set; }

    public ResultValidationStatus ValidationStatus { get; set; } = ResultValidationStatus.Entered;

    public int? ValidatedByStaffId { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public virtual Staff? ValidatedBy { get; set; }

    public virtual TestComponent Component { get; set; } = null!;

    public int? NormalRangeId { get; set; }

    public NormalRange? NormalRange { get; set; }

    public virtual Staff? EnteredByNavigation { get; set; }

    public virtual Staff? LastModifiedByNavigation { get; set; }

    public virtual VisitTest VisitTest { get; set; } = null!;
}
