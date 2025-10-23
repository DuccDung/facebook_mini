namespace chat_service.Models.ModelBase
{
    public class Message_Res
    {
        public Guid message_id { get; set; }
        public string side { get; set; } = null!;
        public string text { get; set; } = null!;
    }
}
