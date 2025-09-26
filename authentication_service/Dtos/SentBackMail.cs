namespace authentication_service.Dtos
{
    public class SentBackMail
    {
        public string Email { get; set; } = string.Empty;
        public int userId { get; set; }
        public string? token { get; set; }
    }
}
