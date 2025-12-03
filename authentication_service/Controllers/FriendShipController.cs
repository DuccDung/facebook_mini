using authentication_service.Models;
using authentication_service.Models.ModelBase;
using authentication_service.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authentication_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendShipController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private readonly FriendshipService _service;
        private readonly HttpClient _http;
        public FriendShipController(AuthenticationContext context, FriendshipService service)
        {
            _context = context;
            _service = service;
            _http = new HttpClient
            {
              //  BaseAddress = new Uri("https://localhost:7070/")
                 BaseAddress = new Uri("http://profile_service:8084/")
            };
        }
        // ========== GỬI LỜI MỜI ==========
        [HttpPost("send")]
        public async Task<IActionResult> SendFriendRequest(int userId, int friendId)
        {
            var ok = await _service.SendFriendRequest(userId, friendId);
            return ok ? Ok("Đã gửi lời mời") : BadRequest("Không thể gửi lời mời");
        }

        // ========== CHẤP NHẬN ==========
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptFriendRequest(Guid friendshipId)
        {
            var ok = await _service.AcceptFriendRequest(friendshipId);
            return ok ? Ok("Đã chấp nhận lời mời") : BadRequest("Không thể chấp nhận");
        }

        // ========== TỪ CHỐI ==========
        [HttpPost("decline")]
        public async Task<IActionResult> DeclineFriendRequest(Guid friendshipId)
        {
            var ok = await _service.DeclineFriendRequest(friendshipId);
            return ok ? Ok("Đã từ chối") : BadRequest("Không thể từ chối");
        }

        // ========== LẤY DANH SÁCH BẠN BÈ ==========
        [HttpGet("friends/{userId}")]
        public async Task<IActionResult> GetFriends(int userId)
        {
            var data = await _service.GetFriends(userId);
            List<FriendRes> res = new List<FriendRes>();
            foreach (var item in data)
            {
                var url = $"api/Profiles/get-profile?userId={item.AccountId}";
                var profile = await _http.GetFromJsonAsync<ProfileRes>(url);
                var check = await _service.isFriend(userId, item.AccountId);
                int count = await _service.GetMutualFriendsCount(userId, item.AccountId);
                res.Add(new FriendRes
                {
                    userId = item.AccountId,
                    UserName = profile != null ? profile.FullName : "",
                    avatarUrl = profile != null ? profile.BackgroundImgUrl : null,
                    mutualFriends = count > 0 ? count : 0,
                });
            }

            return Ok(new {count = data.Count() , info = res});
        }
        [HttpGet("notification_friends/{userId}")]
        public async Task<IActionResult> GetFriendsNotification(int userId)
        {
            try
            {
                var data = await _service.GetFriends(userId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // ========== LẤY LỜI MỜI KẾT BẠN ==========
        [HttpGet("pending/{userId}")]
        public async Task<IActionResult> GetPendingRequests(int userId)
        {
            var data = await _service.GetPendingRequests(userId);
            return Ok(data);
        }
        // ========== LẤY SỐ BẠN BÈ CHUNG ==========
        [HttpGet("mutual")]
        public async Task<IActionResult> GetMutualFriends(int userId, int friendId)
        {
            var count = await _service.GetMutualFriendsCount(userId, friendId);
            return Ok(new { mutualFriends = count });
        }
        [HttpGet]
        [Route("find")]
        public async Task<IActionResult> FindFriendByUsername(int userId, string email)
        {
            var users = await _context.Accounts.Where(x => x.Email.Contains(email)).ToListAsync();
            List<UserRes> res = new List<UserRes>();
            foreach (var item in users)
            {
                var url = $"api/Profiles/get-profile?userId={item.AccountId}";
                var profile = await _http.GetFromJsonAsync<ProfileRes>(url);
                var check = await _service.isFriend(userId , item.AccountId);
                res.Add(new UserRes
                {
                    userId = item.AccountId,
                    Email = item.Email,
                    UserName = profile != null ? profile.FullName : "",
                    avatarUrl = profile != null ? profile.BackgroundImgUrl : null,
                    isFriend = check.isFriend,
                    status = check.status
                });
            }

            return Ok(res);
        }
    }
}
