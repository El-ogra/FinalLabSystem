using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class AuditLog
{
    public long AuditId { get; set; }

    public string TableName { get; set; } = null!;

    public int RecordId { get; set; }

    public string Action { get; set; } = null!;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? SessionInfo { get; set; }

    public string? Notes { get; set; }

    public virtual Staff? ChangedByNavigation { get; set; }
}
