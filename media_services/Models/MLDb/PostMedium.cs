using System;
using System.Collections.Generic;

namespace media_services.Models.MLDb;

public partial class PostMedium
{
    public string MediaId { get; set; }

    public Guid PostId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string? MediaType { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
