namespace media_services.Dtos
{
    public class ProfileListRequest
    {
        public List<int> UserIds { get; set; } = new();
    }

    public class UserProfileDto
    {
        public int UserId { get; set; }
        public Guid ProfileId { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string? AvatarUrl { get; set; }
        public string? CoverImgUrl { get; set; }
        public string? BackgroundImgUrl { get; set; }
    }

}
