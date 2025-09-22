using AuthorizationProto;
using chat_service.Internal;

namespace chat_service.service
{
    public class Authorization : IAuthorization
    {
        private readonly AuthorizationGrpcService.AuthorizationGrpcServiceClient _authorization;
        public Authorization(AuthorizationGrpcService.AuthorizationGrpcServiceClient authorizationClient)
        {
            _authorization = authorizationClient;
        }
        public async Task<bool> CanSendMessageAsync(int userId, Guid conversationId)
        {
            // xử lý
            var request = new RoleConversationRequest
            {
                UserId = userId,
                AssetId = conversationId.ToString()
            };
            var reply = await _authorization.IsRoleConversationAsync(request);
            return reply.Success;
        }

        public Task<bool> IsAdminGroupAsync(int userId, Guid conversationId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserBlockedAsync(int userId, int blockedUserId)
        {
            throw new NotImplementedException();
        }
    }
}
