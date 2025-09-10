namespace chat_service.Models.ModelBase
{
    public class Message_Req
    {
        public Guid ConversationId { get; set; }

        public int SenderId { get; set; }

        public string? Content { get; set; }

        public byte MessageType { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? ParentMessageId { get; set; }

    }
}
