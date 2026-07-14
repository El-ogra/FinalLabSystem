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

    // === Report Layout Settings ===
    public string? ReportLabNameAr { get; set; }
    public string? ReportLabNameEn { get; set; }
    public string? ReportLogoPath { get; set; }
    public decimal? ReportLogoWidth { get; set; }
    public decimal? ReportLogoHeight { get; set; }
    public string? ReportPrimaryColor { get; set; }
    public string? ReportSecondaryColor { get; set; }
    public string? ReportFontFamily { get; set; }
    public double ReportFontSize { get; set; } = 12;
    public double ReportHeaderFontSize { get; set; } = 16;
    public double ReportFooterFontSize { get; set; } = 10;
    public decimal ReportMarginTop { get; set; } = 2;
    public decimal ReportMarginBottom { get; set; } = 2;
    public decimal ReportMarginLeft { get; set; } = 2;
    public decimal ReportMarginRight { get; set; } = 2;
    public bool ReportShowHeader { get; set; } = true;
    public bool ReportShowFooter { get; set; } = true;
    public bool ReportShowStamp { get; set; } = false;
    public string? ReportHeaderText { get; set; }
    public string? ReportFooterText { get; set; }
    public string? ReportPageOrientation { get; set; }
    public string? ReportPaperSize { get; set; }
}
