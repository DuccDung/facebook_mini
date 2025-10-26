using chat_service.Models;
using chat_service.Models.ModelBase;

namespace chat_service.Internal
{
    public interface IConversation
    {
        Task<ResponseModel<cv_res>> CreateConversation1v1(Conversation_Req req);
        Task<ResponseModel<Conversation_Res>> GetConversation(int userId);
        Task<ResponseModel<Conversation_Req>> PutConversation(Conversation conversation);
        Task<ResponseModel<Conversation_Req>> DeleteConversation(Guid conversationId);
        Task<List<MessageModel>> GetMessageHistory(Guid conversationId , int userId);
        Task<string> FormatMessageTime(string utcIsoString);
    }
}
