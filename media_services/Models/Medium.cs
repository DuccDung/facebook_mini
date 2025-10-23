using System;
using System.Collections.Generic;

namespace media_services.Models;

public partial class Medium
{
    public Guid MediaId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public DateTime CreateAt { get; set; }

    public string ObjectKey { get; set; } = null!;

    public double Size { get; set; }

    public string? AssetId { get; set; }
}
