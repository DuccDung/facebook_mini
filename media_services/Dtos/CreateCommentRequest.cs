namespace media_services.Dtos
{
    public class CreateCommentRequest
    {
        public Guid PostId { get; set; }
        public int AccountId { get; set; }
        public string Content { get; set; } = null!;
        public Guid? ParentCommentId { get; set; }  // null = comment cha
    }
}
