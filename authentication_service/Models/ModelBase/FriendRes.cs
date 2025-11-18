namespace authentication_service.Models.ModelBase
{
    public class FriendRes
    {
        public int userId { get; set; }
        public string? UserName { get; set; }
        public string? avatarUrl { get; set; }
        public int mutualFriends { get; set; }
    }
}
