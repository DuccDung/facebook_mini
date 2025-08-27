using infrastructure.caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace authentication_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoCacheController : ControllerBase
    {
        private readonly ICacheService _cache;
        public DemoCacheController(ICacheService cache)
        {
            _cache = cache;
        }
        // POST api/democache/set
        [HttpPost("set")]
        public async Task<IActionResult> Set()
        {
            await _cache.SetAsync("user:1", new { Name = "Alice", Role = "Admin" }, TimeSpan.FromMinutes(5));
            return Ok("Đã set cache với key user:1");
        }

        // GET api/democache/get
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var elem = await _cache.GetAsync<JsonElement>("user:1");
            if (elem.ValueKind == JsonValueKind.Undefined || elem.ValueKind == JsonValueKind.Null)
                return NotFound();

            return Content(elem.GetRawText(), "application/json");
        }

        // DELETE api/democache/remove
        [HttpDelete("remove")]
        public async Task<IActionResult> Remove()
        {
            var removed = await _cache.RemoveAsync("user:1");
            return Ok(new { removed });
        }
    }
}
