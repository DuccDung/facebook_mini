using System;
using System.Collections.Generic;

namespace media_services.Models.MLDb;

public partial class PostComment
{
    public string CommentId { get; set; }

    public Guid PostId { get; set; }

    public string AccountId { get; set; } = null!;

    public string? Content { get; set; }

    public int? ParentCommentId { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
