using authentication_service.Dtos;
using authentication_service.Internal;
using authentication_service.Models;
using infrastructure.caching;
using infrastructure.rabit_mq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

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
        public AuthController(IAuthentication authenticationService, AuthenticationContext context, ITokenService tokenService, ICacheService cache, IRabitMqService mq, IOptionsMonitor<TopologyOption> topos)
        {
            _authenticationService = authenticationService;
            _context = context;
            _tokenService = tokenService;
            _cache = cache;
            _rabitMqService = mq;
            _topos = topos;
        }
        [HttpPost]
        [Route("/api/sign-in")]
        public async Task<IActionResult> Login(LoginData login_Data)
        {
            var result = await _authenticationService.Login(login_Data.acc_info, login_Data.password);
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

        //[HttpGet]
        //[Route("/api/sign-in")]
        //public async Task<IActionResult> SignIn(string name, string email, string password)
        //{
        //    var result = await _authenticationService.SignIn(name, email, password);
        //    if (!result.IsSussess || result.Data == null)
        //    {
        //        return BadRequest(new { result.Message });
        //    }
        //    // publish -> email_service
        //    using var ch = _rabitMqService.CreateChannel();
        //    var topo = _topos.Get("user-registered");
        //    var json = System.Text.Json.JsonSerializer.Serialize(new { email, at = DateTime.UtcNow });
        //    _rabitMqService.Bind(ch, topo);
        //    _rabitMqService.Publish(ch, topo, json);

        //    await Task.CompletedTask;
        //    return Ok(new { Message = "Sign-in pendding confirm email !", name = name });
        //}
        [HttpPost]
        [Route("/api/sign-up")]
        public async Task<IActionResult> SignUp([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (req.Email != req.EmailConfirmation)
            {
                return BadRequest(new { Message = "Email and Email Confirmation do not match." });
            }
            if (await _context.Accounts.AnyAsync(a => a.Email == req.Email))
            {
                return BadRequest(new { Message = "Email is already registered." });
            }

            var res = await _authenticationService.SignUp(req);
            if (!res.IsSussess) return BadRequest(new { res.Message });
            // init token temporarily
            var tokens = await _tokenService.GenerateTokenTemporarily(res.Data.AccountId); // token die affter 1 minutes
            await _cache.SetAsync(res.Data.Email, tokens.AccessToken, TimeSpan.FromHours(0.5)); // save token temporarily 30 minutes
            // pushlish -> setup profile
            using var ch = _rabitMqService.CreateChannel();
            var topo = _topos.Get("setup_profile");
            var env = new Envelope { user_id = res.Data.AccountId, Req = req, At = DateTimeOffset.UtcNow };
            var json = System.Text.Json.JsonSerializer.Serialize(new { env, at = DateTime.UtcNow });
            _rabitMqService.Bind(ch, topo);
            _rabitMqService.Publish(ch, topo, json);
            // publish -> email_service
            var topo_mail = _topos.Get("user-registered");
            var json_mail = System.Text.Json.JsonSerializer.Serialize(new { email = res.Data.Email, token = tokens.AccessToken, at = DateTime.UtcNow });
            _rabitMqService.Bind(ch, topo_mail);
            _rabitMqService.Publish(ch, topo_mail, json_mail);

            return Ok(new
            {
                userId = res.Data.AccountId,
                token = tokens.AccessToken,
                Message = "Sign-up pendding confirm email !"
            });
        }

        [HttpGet]
        [Route("/api/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string email)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null)
            {
                return NotFound(new { Message = "Account not found." });
            }
            account.active = true;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            return Redirect("http://127.0.0.1:8000/");
        }
        [HttpGet]
        [Route("/api/again-sent-email")]
        public async Task<IActionResult> SendBack(int userId,string email, string token)
        {
            var isToken = await _cache.GetAsync<string>(email);
            if (isToken == null || isToken != token)
            {
                return BadRequest("Token is invalid or expired, please sign-up again !");
            }
            // set cache token again
            var token_new = await _tokenService.GenerateTokenTemporarily(userId); // token die affter 1 minutes
            await _cache.SetAsync(email, token_new.AccessToken, TimeSpan.FromHours(0.5)); // save token temporarily 30 minutes
            // publish -> email_service
            var ch = _rabitMqService.CreateChannel();
            var topo_mail = _topos.Get("user-registered");
            var json_mail = System.Text.Json.JsonSerializer.Serialize(new { email = email, token = token_new.AccessToken ,at = DateTime.UtcNow });
            _rabitMqService.Bind(ch, topo_mail);
            _rabitMqService.Publish(ch, topo_mail, json_mail);

            return Ok(new
            {
                token = token_new.AccessToken,
                Message = "Sign-up pendding confirm email !"
            });
        }
    }
}
