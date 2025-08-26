using authentication_service.Internal;
using authentication_service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace authentication_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthentication _authenticationService;
        private readonly ITokenService _tokenService;
        private readonly AuthenticationContext _context;
        public AuthController(IAuthentication authenticationService , AuthenticationContext context, ITokenService tokenService)
        {
            _authenticationService = authenticationService;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpGet]
        [Route("/api/login")]
        public async Task<IActionResult> Login(string acc_info, string password)
        {
            var acc = await _context.Accounts
                .FirstOrDefaultAsync(a => (a.AccountName == acc_info || a.Email == acc_info) && a.Password == password);
            if (acc == null)
            {
                return BadRequest(new { Message = "Invalid account information or password." });
            }
            var response = new Account
            {
                AccountId = acc.AccountId,
                AccountName = acc.AccountName,
                Email = acc.Email,
                PhotoUrl = acc.PhotoUrl
            };
            return Ok(new { Message = "Login endpoint hit", AccountInfo = acc_info });
        }
        [HttpGet]
        [Route("/api/test")]
        public async Task<IActionResult> Test()
        {
            return Ok(new { Message = "Test endpoint hit" });
        }
    }
}
