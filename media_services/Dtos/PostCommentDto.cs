namespace media_services.Dtos
{
    public class PostCommentDto
    {
        public Guid CommentId { get; set; }
        public int AccountId { get; set; }
        public string? avatar { get; set; }
        public string? userName { get; set; }
        public string Content { get; set; } = null!;
        public Guid? ParentCommentId { get; set; }
        public DateTime CreateAt { get; set; }

        // Danh sách reply (comment con)
        public List<PostCommentDto> Replies { get; set; } = new();
    }
}
