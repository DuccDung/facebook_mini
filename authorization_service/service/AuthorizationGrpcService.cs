using Grpc.Core;
using AuthorizationProto;
namespace authorization_service.service
{
    public class AuthorizationGrpcService : AuthorizationProto.AuthorizationGrpcService.AuthorizationGrpcServiceBase
    {
        public override async Task<RoleConversationReply> IsRoleConversation(RoleConversationRequest request , ServerCallContext context)
        {
            await Task.CompletedTask;
            return new RoleConversationReply { Success = true };
        }
    }
}
