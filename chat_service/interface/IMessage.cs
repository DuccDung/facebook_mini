using chat_service.Models;
using chat_service.Models.ModelBase;

namespace chat_service.Internal 
{
    public interface IMessage
    {
        Task<ResponseModel<Message_Req>> CreateMessage(Message_Req req);
        Task<ResponseModel<Message_Req>> GetMessage(Guid messageId);
        Task<ResponseModel<Message_Req>> PutMessage(Message message);
        Task<ResponseModel<Message_Req>> DeleteMessage(Guid messageId);
    }
}
