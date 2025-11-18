namespace authentication_service.Models.ModelBase
{
    public class ProfileRes
    {
        public Guid ProfileId { get; set; }
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? CoverImgUrl { get; set; }
        public string? BackgroundImgUrl { get; set; }
        public int CountFriend { get; set; }
    }
}
