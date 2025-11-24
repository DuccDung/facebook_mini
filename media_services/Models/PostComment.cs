using System;
using System.Collections.Generic;

namespace media_services.Models;

public partial class PostComment
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public int AccountId { get; set; }
    public string Content { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public DateTime CreateAt { get; set; }

    public virtual Post Post { get; set; } = null!;
    public virtual PostComment? ParentComment { get; set; }
    public virtual ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
}
