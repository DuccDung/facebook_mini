using System;
using System.Collections.Generic;

namespace authorization_service.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
	public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
