using System;
using System.Collections.Generic;

namespace authorization_service.Models;

public partial class Asset
{
    public int AssetId { get; set; }

    public int AssetTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? MetaJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AccountAssetRole> AccountAssetRoles { get; set; } = new List<AccountAssetRole>();

    public virtual AssetType AssetType { get; set; } = null!;
}
