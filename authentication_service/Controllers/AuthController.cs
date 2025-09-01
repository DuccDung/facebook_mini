using authentication_service.Internal;
using authentication_service.Models;
using infrastructure.caching;
using infrastructure.rabit_mq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace authentication_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthentication _authenticationService;
        private readonly ITokenService _tokenService;
        private readonly AuthenticationContext _context;
        private readonly ICacheService _cache;
        private readonly IRabitMqService _rabitMqService;
        private readonly IOptionsMonitor<TopologyOption> _topos;
        public AuthController(IAuthentication authenticationService, AuthenticationContext context, ITokenService tokenService , ICacheService cache, IRabitMqService mq, IOptionsMonitor<TopologyOption> topos)
        {
            _authenticationService = authenticationService;
            _context = context;
            _tokenService = tokenService;
            _cache = cache;
            _rabitMqService = mq;
            _topos = topos;
        }
        [HttpGet]
        [Route("/api/login")]
        public async Task<IActionResult> Login(string acc_info, string password)
        {
            var result = await _authenticationService.Login(acc_info, password);
            if (!result.IsSussess || result.Data == null)
            {
                return Unauthorized(new { result.Message });
            }

            var tokens = await _tokenService.GenerateToken(result.Data.AccountId);
            await _cache.SetAsync($"refresh:{tokens.RefreshToken}", result.Data.AccountId, TimeSpan.FromDays(7));
            return Ok(new
            {
                Message = "Login success",
                Account = result.Data,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                Expiration = tokens.Expiration
            });
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken, CancellationToken ct)
        {
            var storedUserId = await _cache.GetAsync<int>($"refresh:{refreshToken}");
            if (storedUserId == 0) 
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            var tokens = await _tokenService.GenerateToken(storedUserId);

            await _cache.RemoveAsync($"refresh:{refreshToken}");
            await _cache.SetAsync($"refresh:{tokens.RefreshToken}", storedUserId, TimeSpan.FromDays(7));

            return Ok(tokens);
        }

        [HttpGet]
        [Route("/api/sign-in")]
        public async Task<IActionResult> SignIn(string name , string email, string password)
        {
            var result = await _authenticationService.SignIn(name, email, password);
            if (!result.IsSussess || result.Data == null)
            {
                return BadRequest(new { result.Message });
            }
            // publish -> email_service
            using var ch = _rabitMqService.CreateChannel();
            var topo = _topos.Get("user-registered");
            var json = System.Text.Json.JsonSerializer.Serialize(new { email, at = DateTime.UtcNow });
            _rabitMqService.Bind(ch, topo);
            _rabitMqService.Publish(ch, topo, json);

            await Task.CompletedTask;
            return Ok(new { Message = "Sign-in pendding confirm email !", name = name });
        }
    }
}
