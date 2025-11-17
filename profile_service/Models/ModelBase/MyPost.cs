namespace profile_service.Models.ModelBase
{
    public class MyPost
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<string>? AssetUrls { get; set; }
        public List<Like>? likes { get; set; }
        //public List<Like>? Comment { get; set; }
    }
}
