using notification_service.Models.Dtos;
using System.Net.WebSockets;
using System.Text;

namespace notification_service.service
{
    public static class WebSocketHandler
    {
        public static Dictionary<string, WebSocket> ConnectedUsers = new();

        public static async Task HandleAsync(WebSocket socket, string userId) // handle connect ws
        {
            ConnectedUsers[userId] = socket;

            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                }
            }
            finally
            {
                ConnectedUsers.Remove(userId);
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
        }
        public static async Task SendToUsersAsync(mes_notification notification)
        {
            var userIds = notification.receiver_ids;
            var message = new notification_ws
            {
                sender = notification.sender,
                content = notification.content,
                type = notification.type,
                avatar_url = notification.avatar_url,
                created_at = notification.created_at
            };
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            var data = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(data);
            foreach (var userId in userIds)
            {
                Console.WriteLine(">>> Sending notification to userId: " + userId);
                var tempId = userId.ToString();
                if (!ConnectedUsers.TryGetValue(tempId, out var socket)) continue;
                if (socket.State != WebSocketState.Open) { ConnectedUsers.Remove(tempId); continue; }
                try { await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None); }
                catch { ConnectedUsers.Remove(tempId); Console.WriteLine("lỗi gửi ws!"); }
            }
        }
    }

}
