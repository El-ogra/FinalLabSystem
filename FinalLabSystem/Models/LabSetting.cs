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
}
