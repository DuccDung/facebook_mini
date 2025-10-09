using chat_service.Models;
using chat_service.Models.ModelBase;

namespace chat_service.Internal
{
    public interface IConversation
    {
        Task<ResponseModel<Conversation_Req>> CreateConversation(Conversation_Req req);
        Task<ResponseModel<Models.Conversation>> GetConversation(int userId);
        Task<ResponseModel<Conversation_Req>> PutConversation(Conversation conversation);
        Task<ResponseModel<Conversation_Req>> DeleteConversation(Guid conversationId);
    }
}
