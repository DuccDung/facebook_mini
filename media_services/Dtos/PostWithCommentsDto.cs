using System;
using System.Collections.Generic;

namespace media_services.Dtos
{
    public class PostWithCommentsDto
    {
        public Guid PostId { get; set; }
        public int AccountId { get; set; }
        public string? avatar { get; set; }
        public string? userName { get; set; }
        public string? Content { get; set; }
        public byte PostType { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public PostLikeInfoDto InforLike {  get; set; }
        public List<MediaItemDto> MediaItems { get; set; } = new();
        // Có thể thêm các field khác: likeCount, shareCount, ...
        public List<PostCommentDto> Comments { get; set; } = new();
    }
}
