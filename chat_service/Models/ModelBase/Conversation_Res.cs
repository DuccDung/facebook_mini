namespace chat_service.Models.ModelBase
{
    public class Conversation_Res
    {
        public Guid ConversationId { get; set; }
        public string ConversationName { get; set; } = string.Empty;
        public string PhotoUrl { get; set;} = string.Empty;
    }
}
