using System;
using System.Collections.Generic;

namespace media_services.Models.MLDb;

public partial class Like
{
    public string LikeId { get; set; }

    public Guid PostId { get; set; }

    public string AccountId { get; set; } = null!;

    public string? LikeType { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
