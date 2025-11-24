using System;
using System.Collections.Generic;

namespace media_services.Models.MLDb;

public partial class Post
{
    public Guid PostId { get; set; }

    public string AccountId { get; set; } = null!;

    public string? Content { get; set; }

    public string? PostType { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? IsRemove { get; set; }

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    public virtual ICollection<PostMedium> PostMedia { get; set; } = new List<PostMedium>();

    public virtual ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();
}
