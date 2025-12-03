using System.Net.WebSockets;

namespace notification_service.Models.Dtos
{
    public class User
    {
        public string Username { get; set; }
        public string Status { get; set; } // "online", "offline"
        public WebSocket Connection { get; set; } // WebSocket connection for this user
    }

}
