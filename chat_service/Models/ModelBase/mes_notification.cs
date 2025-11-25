namespace chat_service.Models.ModelBase
{
    public class mes_notification
    {
        public List<int> receiver_ids { get; set; } = new List<int>();
        public user sender { get; set; } = new user();
        public string content { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string avatar_url { get; set; } = string.Empty;
        public string asset_id { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
    }
}
