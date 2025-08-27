using authorization_service.Dtos;
using authorization_service.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace authorization_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authz;

        public AuthorizationController(IAuthorizationService authz)
        {
            _authz = authz;
        }
        // GET api/authorization/check?userId=1&assetId=10&permission=POST_EDIT
        [HttpGet("check")]
        public async Task<IActionResult> CheckPermission(int userId, int assetId, string permission, CancellationToken ct)
        {
            var has = await _authz.HasPermissionAsync(userId, assetId, permission, ct);
            return Ok(new { UserId = userId, AssetId = assetId, Permission = permission, HasPermission = has });
        }
        [HttpPost("user-asset-roles")]
        public async Task<IActionResult> CreateUserAssetRole([FromBody] CreateUserAssetRoleRequest req, CancellationToken ct)
        {
            await _authz.GrantRoleAsync(req.GrantedBy, req.UserId, req.AssetId, req.RoleId, ct);
            return Ok(new { Message = "Role granted successfully" });
        }
    }
}
