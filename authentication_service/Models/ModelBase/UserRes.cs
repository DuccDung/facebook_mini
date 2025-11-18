namespace authentication_service.Models.ModelBase
{
    public class UserRes
    {
        public int userId { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? avatarUrl { get; set; }

        public bool isFriend { get; set; }
        public string? status { get; set; }
    }
}
