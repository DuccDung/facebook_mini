namespace notification_service.Models.Dtos
{
    public class user
    {
        public int userId { get; set; }
        public string? username { get; set; }
    }
    public class mes_notification
    {
        public List<int> receiver_ids { get; set; } = new List<int>();
        public user sender { get; set; } = new user();
        public string content { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string avatar_url { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
    public class notification_ws
    {
        public user sender { get; set; } = new user();
        public string content { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string avatar_url { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
