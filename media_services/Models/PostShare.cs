using System;

namespace media_services.Models;

public partial class PostShare
{
    public Guid PsId { get; set; }
    public Guid PostId { get; set; }
    public int AccountId { get; set; }

    public virtual Post Post { get; set; } = null!;
}
