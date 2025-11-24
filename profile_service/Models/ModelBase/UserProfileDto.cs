namespace profile_service.Models.ModelBase
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public Guid ProfileId { get; set; }
        public string FullName { get; set; }
        public string? Bio { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        public string? AvatarUrl { get; set; }
        public string? CoverImgUrl { get; set; }
        public string? BackgroundImgUrl { get; set; }
    }

}
