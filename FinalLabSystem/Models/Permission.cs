using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string PermissionCode { get; set; } = null!;

    public string PermissionName { get; set; } = null!;

    public string PermissionGroup { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<StaffPermission> StaffPermissions { get; set; } = new List<StaffPermission>();
}
