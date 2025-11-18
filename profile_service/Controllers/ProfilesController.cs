using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using profile_service.Models;
using profile_service.Models.ModelBase;

namespace profile_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly ProfileContext _context;
        private readonly HttpClient _http;
        public ProfilesController(ProfileContext context)
        {
            _context = context;
            _http = new HttpClient
            {
                //BaseAddress = new Uri("https://localhost:7121/")
                BaseAddress = new Uri("http://media_service:8086/")
            };
        }
        [HttpGet]
        [Route("get-profile")]
        public async Task<IActionResult> GetProfileByUserId(int userId)
        {
            var profile =await _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound("null profile");

            var url = $"api/Media/get/by-asset?asset_id={profile.ProfileId.ToString()}";
            var media = await _http.GetFromJsonAsync<List<MediaItemDto>>(url);
            var profile_res = new ProfileRes()
            {
                UserId = userId,
                ProfileId = profile.ProfileId,
                FullName = profile.FullName,
                Bio = profile.Bio,
                DateOfBirth = profile.DateOfBirth,
            };
            foreach (var item in media ?? new List<MediaItemDto>())
            {
                if (item.MediaType == "cover_image")
                {
                    profile_res.CoverImgUrl = item.MediaUrl;
                }
                else if(item.MediaType == "background_image")
                {
                    profile_res.BackgroundImgUrl = item.MediaUrl;
                }
            }
            return Ok(profile_res);
        }
        [HttpPost]
        [Route("create-cover-image")]
        public async Task<IActionResult> CreateCoverImg(string url, string profile_id)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId.ToString() == profile_id);
            if (profile == null) return NotFound("null profile");
            profile.AvartaUrl = url;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok("success");
        }
        [HttpPost]
        [Route("update-cover-image")]
        public async Task<IActionResult> UpdateCoverImg(string url, string profile_id)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileId.ToString() == profile_id);
            if (profile == null) return NotFound("null profile");
            profile.AvartaUrl = url;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return Ok("success");
        }

    }
}
