namespace media_services.Dtos
{
    public class CreatePostRequest
    {
        public int UserId { get; set; }
        public string? Content { get; set; }
        public byte PostType { get; set; } = 0;   // 0 = normal text, 1=image... tùy bạn
        public List<int>? TagFriendIds { get; set; } = new();
    }
}
