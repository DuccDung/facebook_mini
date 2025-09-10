namespace chat_service.Models.ModelBase
{
    public class ConversationMember_Req
    {
        public Guid ConversationId { get; set; }

        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; }

        public string? Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool? Remove { get; set; }
    }
}
