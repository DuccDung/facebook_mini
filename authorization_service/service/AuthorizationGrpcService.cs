using Grpc.Core;
using AuthorizationProto;
using authorization_service.Internal;

namespace authorization_service.service
{
	public class AuthorizationGrpcService : AuthorizationProto.AuthorizationGrpcService.AuthorizationGrpcServiceBase
	{
		private readonly IAuthorizationService _authorizationService;
		private readonly ILogger<AuthorizationGrpcService> _logger;

		public AuthorizationGrpcService(
			IAuthorizationService authorizationService,
			ILogger<AuthorizationGrpcService> logger)
		{
			_authorizationService = authorizationService;
			_logger = logger;
		}

		// Helper parse int
		private bool TryParseIds(string userIdRaw, string assetIdRaw, out int userId, out int assetId, out string error)
		{
			error = string.Empty;
			if (!int.TryParse(userIdRaw, out userId))
			{
				error = "Invalid userId";
				assetId = 0;
				return false;
			}
			if (!int.TryParse(assetIdRaw, out assetId))
			{
				error = "Invalid assetId";
				return false;
			}
			return true;
		}

		// RPC cũ
		public override async Task<RoleConversationReply> IsRoleConversation(RoleConversationRequest request, ServerCallContext context)
		{
			try
			{
				if (!TryParseIds(request.UserId, request.AssetId, out var userId, out var assetId, out var parseErr))
				{
					return new RoleConversationReply
					{
						Success = false,
						Message = parseErr,
						Role = ""
					};
				}

				var roles = await _authorizationService.GetUserRolesOnAssetAsync(userId, assetId, context.CancellationToken);
				if (roles.Count == 0)
				{
					return new RoleConversationReply
					{
						Success = false,
						Message = "User has no role on this asset",
						Role = ""
					};
				}

				return new RoleConversationReply
				{
					Success = true,
					Message = "User has role(s)",
					Role = string.Join(",", roles)
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in IsRoleConversation");
				return new RoleConversationReply
				{
					Success = false,
					Message = "Internal server error",
					Role = ""
				};
			}
		}

		// NEW: HasPermission
		public override async Task<HasPermissionReply> HasPermission(HasPermissionRequest request, ServerCallContext context)
		{
			try
			{
				if (!TryParseIds(request.UserId, request.AssetId, out var userId, out var assetId, out var parseErr))
				{
					return new HasPermissionReply
					{
						Success = false,
						HasPermission = false,
						Message = parseErr
					};
				}

				if (string.IsNullOrWhiteSpace(request.PermissionCode))
				{
					return new HasPermissionReply
					{
						Success = false,
						HasPermission = false,
						Message = "permissionCode is required"
					};
				}

				var has = await _authorizationService.HasPermissionAsync(userId, assetId, request.PermissionCode, context.CancellationToken);

				return new HasPermissionReply
				{
					Success = true,
					HasPermission = has,
					Message = has ? "Permission granted" : "Permission denied"
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in HasPermission");
				return new HasPermissionReply
				{
					Success = false,
					HasPermission = false,
					Message = "Internal server error"
				};
			}
		}

		// NEW: GetPermissions
		public override async Task<GetPermissionsReply> GetPermissions(GetPermissionsRequest request, ServerCallContext context)
		{
			var reply = new GetPermissionsReply();
			try
			{
				if (!TryParseIds(request.UserId, request.AssetId, out var userId, out var assetId, out var parseErr))
				{
					_logger.LogWarning("GetPermissions parse error: {err}", parseErr);
					return reply;
				}

				var perms = await _authorizationService.GetEffectivePermissionsAsync(userId, assetId, context.CancellationToken);
				reply.Permissions.AddRange(perms);
				return reply;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GetPermissions");
				return reply; // trả về rỗng nếu lỗi
			}
		}

		// NEW: GrantRole
		public override async Task<GrantRoleReply> GrantRole(GrantRoleRequest request, ServerCallContext context)
		{
			try
			{
				if (!int.TryParse(request.GrantedByUserId, out var grantedBy))
					return new GrantRoleReply { Success = false, Message = "Invalid grantedByUserId" };

				if (!TryParseIds(request.UserId, request.AssetId, out var userId, out var assetId, out var parseErr))
					return new GrantRoleReply { Success = false, Message = parseErr };

				if (!int.TryParse(request.RoleId, out var roleId))
					return new GrantRoleReply { Success = false, Message = "Invalid roleId" };

				await _authorizationService.GrantRoleAsync(grantedBy, userId, assetId, roleId, context.CancellationToken);

				var roles = await _authorizationService.GetUserRolesOnAssetAsync(userId, assetId, context.CancellationToken);
				return new GrantRoleReply
				{
					Success = true,
					Message = "Role granted (or already existed)",
					UpdatedRoles = { roles }
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in GrantRole");
				return new GrantRoleReply
				{
					Success = false,
					Message = "Internal server error"
				};
			}
		}

		// NEW: RevokeRole
		public override async Task<RevokeRoleReply> RevokeRole(RevokeRoleRequest request, ServerCallContext context)
		{
			try
			{
				if (!TryParseIds(request.UserId, request.AssetId, out var userId, out var assetId, out var parseErr))
					return new RevokeRoleReply { Success = false, Message = parseErr };

				if (!int.TryParse(request.RoleId, out var roleId))
					return new RevokeRoleReply { Success = false, Message = "Invalid roleId" };

				await _authorizationService.RevokeRoleAsync(userId, assetId, roleId, context.CancellationToken);

				var roles = await _authorizationService.GetUserRolesOnAssetAsync(userId, assetId, context.CancellationToken);
				return new RevokeRoleReply
				{
					Success = true,
					Message = "Role revoked (if existed)",
					UpdatedRoles = { roles }
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in RevokeRole");
				return new RevokeRoleReply
				{
					Success = false,
					Message = "Internal server error"
				};
			}
		}
	}
}