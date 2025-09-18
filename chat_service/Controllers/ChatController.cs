using Microsoft.AspNetCore.Mvc;
using AuthorizationProto;

namespace chat_service.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ChatController : ControllerBase
	{
		private readonly AuthorizationGrpcService.AuthorizationGrpcServiceClient _authzGrpc;

		public ChatController(AuthorizationGrpcService.AuthorizationGrpcServiceClient authzGrpc)
		{
			_authzGrpc = authzGrpc;
		}

		// GET: api/chat/permissions?userId=1&assetId=10
		[HttpGet("permissions")]
		public async Task<IActionResult> GetPermissions([FromQuery] string userId, [FromQuery] string assetId)
		{
			var request = new GetPermissionsRequest
			{
				UserId = userId,
				AssetId = assetId
			};

			var reply = await _authzGrpc.GetPermissionsAsync(request);

			return Ok(new
			{
				UserId = userId,
				AssetId = assetId,
				Permissions = reply.Permissions.ToArray()
			});
		}
	}
}
