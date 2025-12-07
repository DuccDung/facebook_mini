using Newtonsoft.Json;
using System.Text.Json.Serialization;
namespace notification_service.Models.Dtos
{
    public class CallMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("callType")]
        public string CallType { get; set; }

        [JsonPropertyName("conversation_id")]
        public string ConversationId { get; set; }
    }
    public class CallMessageResponse
    {
        public string type { get; set; } // Loại thông điệp (calling)
        public string SenderId { get; set; } // userId
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string ConversationId { get; set; } // conversation_id
    }

}   
