using System;
using System.Collections.Generic;
namespace media_services.Models
{
    public partial class Post
    {
        public Guid PostId { get; set; }
        public int AccountId { get; set; }
        public string? Content { get; set; }
        public byte PostType { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsRemove { get; set; }

        public virtual ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();
        public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    }
}
