
using authentication_service.Models;
using notification_service.Models;
using notification_service.Models.Dtos;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;

namespace notification_service.service
{
    public class StatusOperationFriendService 
    {
        private HttpClient auth;
        public StatusOperationFriendService() {
           auth = new HttpClient
           {
               //BaseAddress = new Uri("https://localhost:7070/")
               BaseAddress = new Uri("http://authentication_service:8080/")
           };
        }
        public async Task ManageGroupStatus(int userId)
        {
            while (true)
            {
                // api/FriendShip/notification_friends/2053
                var res = await auth.GetAsync($"api/FriendShip/notification_friends/{userId}");
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine("Lỗi khi lấy danh sách bạn bè."); return;
                }
                var friends = await res.Content.ReadFromJsonAsync<List<Account>>();
                foreach (var friend in friends ?? new List<Account>())
                {
                    var tempId = friend.AccountId.ToString();
                    if (!WebSocketHandler.ConnectedUsers.TryGetValue(tempId, out var socket)) continue;
                    var message = new
                    {
                        friendId = friend.AccountId.ToString(),
                        status = "online",
                        type = "status_friend"
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(message);
                    var data = Encoding.UTF8.GetBytes(json);
                    var buffer = new ArraySegment<byte>(data);

                    if (socket.State != WebSocketState.Open) { WebSocketHandler.ConnectedUsers.Remove(tempId); continue; }
                    try { await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None); }
                    catch { WebSocketHandler.ConnectedUsers.Remove(tempId); Console.WriteLine("lỗi gửi ws!"); }
                }
                if (!WebSocketHandler.ConnectedUsers.TryGetValue(userId.ToString(), out var x)) return;
                await Task.Delay(3000);
            }
        }
    }
}
