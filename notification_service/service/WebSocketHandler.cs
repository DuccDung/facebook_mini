using notification_service.Models.Dtos;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using notification_service.Models;

namespace notification_service.service
{
    public static class WebSocketHandler
    {
        // Dùng ConcurrentDictionary cho thread-safe
        public static ConcurrentDictionary<string, WebSocket> ConnectedUsers = new();

        public static async Task HandleAsync(WebSocket socket, string userId) // handle connect ws
        {
            ConnectedUsers[userId] = socket;

            // chạy background task riêng của bạn
            _ = Task.Run(async () =>
            {
                var statusService = new StatusOperationFriendService();
                await statusService.ManageGroupStatus(int.Parse(userId));
            });

            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var sb = new StringBuilder();

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None
                        );

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            // client đóng kết nối
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                            ConnectedUsers.TryRemove(userId, out _);
                            return;
                        }

                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage); // lặp đến khi hết 1 message

                    var msg = sb.ToString();

                    // route message theo "to"
                    //await RouteMessageAsync(msgText, userId);
                    await RouteMessageVideoAsync(msg, userId);
                }
            }
            finally
            {
                ConnectedUsers.TryRemove(userId, out _);
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// Route message: nếu có field "to" thì gửi đúng user đó, không thì broadcast.
        /// </summary>
        private static async Task RouteMessageAsync(string message, string senderId)
        {
            try
            {
                using var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                if (root.TryGetProperty("to", out var toProp))
                {
                    var toUserId = toProp.GetString();

                    if (!string.IsNullOrWhiteSpace(toUserId) &&
                        ConnectedUsers.TryGetValue(toUserId, out var socket) &&
                        socket.State == WebSocketState.Open)
                    {
                        var data = Encoding.UTF8.GetBytes(message);
                        var buffer = new ArraySegment<byte>(data);

                        await socket.SendAsync(
                            buffer,
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            cancellationToken: CancellationToken.None
                        );

                        Console.WriteLine($"Signal message routed {senderId} -> {toUserId}");
                        return;
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parse error in RouteMessageAsync: {ex.Message}");
                // nếu lỗi JSON, vẫn broadcast cho debug
            }

            // fallback: gửi tất cả (trừ sender)
            await SendToAllClientsAsync(message, senderId);
        }
        private static async Task RouteMessageVideoAsync(string message, string senderId)
        {
            try
            {
                using var doc = JsonDocument.Parse(message);
                //var root = doc.RootElement;

                // Chuyển đổi JsonDocument thành CallMessage với kiểm tra null
                CallMessage? callMessage = JsonSerializer.Deserialize<CallMessage>(doc.RootElement.ToString());

                if (callMessage != null)
                {
                    using var client = new HttpClient
                    {
                         BaseAddress = new Uri("http://chat_service:8083/")
                        //13.112.144.107
                        //https://localhost:7253/
                       // BaseAddress = new Uri("http://13.112.144.107:5004/")
                        //BaseAddress = new Uri("https://localhost:7253/")
                    };
                    var res = await client.GetAsync("api/Chat/users?conversationId=" + callMessage.ConversationId);
                    if (!res.IsSuccessStatusCode) return;
                    var user_profiles = await res.Content.ReadFromJsonAsync<List<user_profile>>();
                    var sender = user_profiles?.Where(x => x.userId.ToString() == senderId).FirstOrDefault();
                    foreach (var item in user_profiles ?? new List<user_profile>())
                    {
                        var userId = item.userId.ToString();

                        // Kiểm tra nếu userId khác senderId và người dùng đang kết nối WebSocket
                        if (userId != senderId && ConnectedUsers.TryGetValue(userId, out var socket) && socket.State == WebSocketState.Open)
                        {
                            CallMessageResponse res_call = new CallMessageResponse
                            {
                                type = callMessage.Type,
                                Avatar = sender?.avatar ?? "",
                                SenderId = senderId,
                                ConversationId = callMessage.ConversationId,
                                UserName = sender?.username ?? ""
                            };
                            string jsonResponse = JsonSerializer.Serialize(res_call);
                            if(res_call.type == "calling")
                            {
                                var _data = Encoding.UTF8.GetBytes(jsonResponse);
                                var _buffer = new ArraySegment<byte>(_data);
                                try
                                {
                                    await socket.SendAsync(_buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                    Console.WriteLine($"Message sent to {userId}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error sending message to {userId}: {ex.Message}");
                                    ConnectedUsers.TryRemove(userId, out _);  // Xử lý khi không gửi được
                                }
                            }
                            else
                            {
                                var data = Encoding.UTF8.GetBytes(message);
                                var buffer = new ArraySegment<byte>(data);
                                try
                                {
                                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                                    Console.WriteLine($"Message sent to {userId}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error sending message to {userId}: {ex.Message}");
                                    ConnectedUsers.TryRemove(userId, out _);  // Xử lý khi không gửi được
                                }
                            }
                        }
                    }

                    // Process the callMessage object as needed
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parse error in RouteMessageAsync: {ex.Message}");
                // nếu lỗi JSON, vẫn broadcast cho debug
            }
        }
        // Hàm notification cũ của bạn, giữ nguyên
        public static async Task SendToUsersAsync(mes_notification notification, string notification_id)
        {
            var userIds = notification.receiver_ids;
            var message = new notification_ws
            {
                notification_id = notification_id,
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
                var tempId = userId.ToString();
                if (!ConnectedUsers.TryGetValue(tempId, out var socket)) continue;
                if (socket.State != WebSocketState.Open) { ConnectedUsers.TryRemove(tempId, out _); continue; }

                try
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    ConnectedUsers.TryRemove(tempId, out _);
                    Console.WriteLine("lỗi gửi ws!");
                }
            }
        }

        // Giữ broadcast dùng chung (fallback)
        public static async Task SendToAllClientsAsync(string message, string senderId)
        {
            var data = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(data);

            foreach (var kvp in ConnectedUsers)
            {
                var userId = kvp.Key;
                var socket = kvp.Value;

                if (userId == senderId) continue;
                if (socket.State != WebSocketState.Open)
                {
                    ConnectedUsers.TryRemove(userId, out _);
                    continue;
                }

                try
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine($"Message broadcast to {userId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to {userId}: {ex.Message}");
                    ConnectedUsers.TryRemove(userId, out _);
                }
            }
        }
    }
}
