using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using notification_service.Models;
using notification_service.Models.Dtos;
using static System.Net.WebRequestMethods;

namespace notification_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationDbContext _context;
        private readonly HttpClient _profile;
        private readonly HttpClient _media;

        public NotificationsController(NotificationDbContext context)
        {
            _context = context;
            _profile = new HttpClient
            {
                //  BaseAddress = new Uri("https://localhost:7070/")
                BaseAddress = new Uri("http://profile_service:8084/")
            };
            _media = new HttpClient
            {
                // BaseAddress = new Uri("https://localhost:7121/")
                BaseAddress = new Uri("http://media_service:8086/")
            };
        }
        [HttpGet]
        [Route("Notifications")]
        public async Task<IActionResult> GetNotificationOfTheUser([FromQuery] int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.ReceiverId == userId && n.IsRead == false)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new notification_ws
                {
                    //avatar_url = n.Avatar_Url,
                    content = n.content,
                    created_at = n.CreatedAt,
                    sender = new user
                    {
                        userId = n.ActorId,
                        //username = n.Sender_Username
                    },
                    type = n.Type
                }).ToListAsync();

            foreach (var item in notifications)
            {
                var url = $"api/Profiles/get-profile?userId={item.sender.userId}";
                var profile = await _profile.GetFromJsonAsync<ProfileRes>(url);
                if (profile == null) continue;
                item.sender.username = profile.FullName;
                var url_media = $"api/Media/get/by-asset?asset_id={profile.ProfileId.ToString()}";
                var media = await _media.GetFromJsonAsync<List<MediaItemDto>>(url_media);
                foreach (var avatar in media ?? new List<MediaItemDto>())
                {
                    if (avatar.MediaType != "background_image") continue;
                    item.avatar_url = avatar.MediaUrl;
                    break;
                }
            }

            return Ok(notifications);
        }
        [HttpPost]
        [Route("mark-as-read")]
        public async Task<IActionResult> MarkNotificationsAsRead([FromBody] List<string> notificationIds)
        {
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.NotificationId.ToString()))
                .ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return Ok("Notifications marked as read.");
        }
    }
}
