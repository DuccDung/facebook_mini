using System;
using System.Collections.Generic;

namespace authorization_service.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public int AssetTypeId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    public virtual ICollection<AccountAssetRole> AccountAssetRoles { get; set; } = new List<AccountAssetRole>();

    public virtual AssetType AssetType { get; set; } = null!;

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

	public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
