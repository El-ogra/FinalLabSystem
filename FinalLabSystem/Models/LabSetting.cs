using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class LabSetting
{
    public string SettingKey { get; set; } = null!;

    public string? SettingValue { get; set; }

    public string? SettingDescription { get; set; }

    public string? SettingGroup { get; set; }

    public bool IsRequired { get; set; }

    public int? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public virtual Staff? LastUpdatedByNavigation { get; set; }

    public bool EnforceStageGating { get; set; } = true;
    public bool EnableServerPrinting { get; set; } = false;

    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }
    public bool? SmtpEnableSsl { get; set; }
    public int? BackupScheduleHour { get; set; }
    public int? BackupRetentionDays { get; set; }
    public string? BackupOutputFolder { get; set; }
}
