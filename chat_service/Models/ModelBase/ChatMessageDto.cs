using System;
using System.Text.Json.Serialization;
namespace chat_service.Models.ModelBase
{
    public sealed class ChatMessageDto
    {
        // "text": "heloo"
        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;

        [JsonPropertyName("senderId")]
        public string? SenderId { get; set; }

        // System.Text.Json tự parse Guid từ chuỗi
        [JsonPropertyName("threadId")]
        public Guid ThreadId { get; set; }

        // "createdAt": "2025-10-27T03:25:45.882Z"
        // Nên dùng DateTimeOffset để giữ được múi giờ (Z = UTC)
        [JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }
    }

}
