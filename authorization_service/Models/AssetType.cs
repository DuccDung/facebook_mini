using System;
using System.Collections.Generic;

namespace authorization_service.Models;

public partial class AssetType
{
    public int AssetTypeId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
