using chat_service.Internal;
using chat_service.Models;
using chat_service.Models.ModelBase;
using Microsoft.EntityFrameworkCore;

namespace chat_service.service
{
    public class Conversation : IConversation
    {
        private readonly TextingServicesContext _context;
        public Conversation(TextingServicesContext context)
        {
            _context = context;
        }
        public async Task<ResponseModel<Conversation_Req>> CreateConversation(Conversation_Req req)
        {
            Models.Conversation conversation = new Models.Conversation
            {
                CreatedAt = DateTime.Now,
                IsGroup = req.IsGroup,
                Remove = false,
                Title = req.Title,
            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var result = new Conversation_Req
            {
                UserId = req.UserId,
                CreatedAt = req.CreatedAt,
                IsGroup = req.IsGroup,
                Title = req.Title,
            };

            return new ResponseModel<Conversation_Req>
            {
                IsSussess = true,
                StatusCode = 200,
                Message = "init success",
                Data = result
            };

        }

        public async Task<ResponseModel<Conversation_Req>> DeleteConversation(Guid conversationId)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null) return new ResponseModel<Conversation_Req> { IsSussess = false, StatusCode = 404, Data = null, Message = "not found conversation in database!" };
            _context.Conversations.Remove(conversation);
            await _context.SaveChangesAsync();
            return new ResponseModel<Conversation_Req> { IsSussess = true, StatusCode = 200, Message = "remove success conversation in database!" };
        }

        public async Task<ResponseModel<Models.Conversation>> GetConversation(int userId)
        {
            var conversations =await _context.ConversationMembers.Include(x => x.Conversation).Where(x => x.UserId == userId).ToListAsync();
            foreach (var conversation in conversations)
            {

            }
            var result = new ResponseModel<Models.Conversation>
            {
                //IsSussess = true,
                //StatusCode = 200,
                //Data = new Conversation_Req { CreatedAt = conversation.CreatedAt, IsGroup = conversation.IsGroup, Title = conversation.Title }
            };
            return result;
        }

        public Task<ResponseModel<Conversation_Req>> PutConversation(Models.Conversation conversation)
        {
            throw new NotImplementedException();
        }
    }
}
