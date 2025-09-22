using chat_service.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace chat_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IAuthorization authorization;
        public ValuesController(IAuthorization authorization)
        {
            this.authorization = authorization;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var guid = Guid.NewGuid();
            var can = await authorization.CanSendMessageAsync(1,guid);
            return Ok(new { can });
        }
    }
}
