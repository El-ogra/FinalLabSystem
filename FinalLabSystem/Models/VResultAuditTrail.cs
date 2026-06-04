using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class VResultAuditTrail
{
    public long AuditId { get; set; }

    public int ResultId { get; set; }

    public string Action { get; set; } = null!;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ChangedAt { get; set; }

    public string ChangedByName { get; set; } = null!;

    public int VisitId { get; set; }

    public string VisitCode { get; set; } = null!;

    public string PatientName { get; set; } = null!;

    public string? TestType { get; set; }

    public string? ComponentName { get; set; }
}
