namespace chat_service.Models.ModelBase
{
    public class Conversation_Req
    {
        public int UserId { get; set; }
        public int FriendId { get; set; }
        public bool IsGroup { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class ConversationList_Req
    {
        public int UserId { get; set; }
        public List<int> FriendIds { get; set; } = new List<int>();
        public bool IsGroup { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
