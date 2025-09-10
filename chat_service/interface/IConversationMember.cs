using chat_service.Models.ModelBase;

namespace chat_service.Internal 
{
    interface IConversationMember
    {
        Task<ResponseModel<ConversationMember_Req>> AddMember(ConversationMember_Req req);
        Task<ResponseModel<List<ConversationMember_Req>>> GetMembers(Guid conversationId);
        Task<ResponseModel<ConversationMember_Req>> RemoveMember(Guid conversationId, int userId);
    }
}
