using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class StaffPermission
{
    public int StaffPermId { get; set; }

    public int StaffId { get; set; }

    public int PermissionId { get; set; }

    public bool IsGranted { get; set; }

    public int? GrantedBy { get; set; }

    public DateTime GrantedAt { get; set; }

    public virtual Staff? GrantedByNavigation { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
