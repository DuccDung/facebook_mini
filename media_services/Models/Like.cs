using System;
using System.Collections.Generic;

namespace media_services.Models;

public partial class Like
{
    public Guid LikeId { get; set; }

    public Guid AssetId { get; set; }

    public int AccountId { get; set; }

    public byte LikeType { get; set; }

    public DateTime CreateAt { get; set; }
}
