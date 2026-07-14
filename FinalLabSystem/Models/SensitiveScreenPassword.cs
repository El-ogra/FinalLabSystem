using FinalLabSystem.Data;

namespace FinalLabSystem.Models;

[Auditable]
public class SensitiveScreenPassword
{
    public int SensitiveScreenPasswordId { get; set; }
    public string ScreenType { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int? LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedAt { get; set; }

    public virtual Staff? LastUpdatedByNavigation { get; set; }
}
