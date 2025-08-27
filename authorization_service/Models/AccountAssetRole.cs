using System;
using System.Collections.Generic;

namespace authorization_service.Models;

public partial class AccountAssetRole
{
    public int UserId { get; set; }

    public int AssetId { get; set; }

    public int RoleId { get; set; }

    public int? GrantedBy { get; set; }

    public DateTime GrantedAt { get; set; }

    public virtual Asset Asset { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
