using chat_service.Internal;
using chat_service.Models;
using chat_service.Models.ModelBase;
using MediaProto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace chat_service.service
{
    public class Conversation : IConversation
    {
        private readonly TextingServicesContext _context;
        private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpc;
        public Conversation(TextingServicesContext context, MediaGrpcService.MediaGrpcServiceClient mediaGrpc)
        {
            _context = context;
            _mediaGrpc = mediaGrpc;
        }
        public async Task<ResponseModel<cv_res>> CreateConversation1v1(Conversation_Req req)
        {
            var check = await _context.ConversationMembers.Where(x => x.UserId == req.UserId || x.UserId == req.FriendId)
                                                           .Include(x => x.Conversation)
                                                           .GroupBy(x => x.ConversationId)
                                                           .AnyAsync(x => x.Count() == 2);
            if (!check)
            {
                var cv = new Models.Conversation()
                {
                    CreatedAt = DateTime.UtcNow,
                    IsGroup = false,
                    Title = req.Title,
                };
                await _context.Conversations.AddAsync(cv);
                await _context.SaveChangesAsync();

                ConversationMember member_1 = new ConversationMember()
                {
                    UserId = req.UserId,
                    ConversationId = cv.ConversationId,
                };
                await _context.ConversationMembers.AddAsync(member_1);
                ConversationMember member_2 = new ConversationMember() { UserId = req.FriendId, ConversationId = cv.ConversationId, };
                _context.ConversationMembers.Add(member_2);
                await _context.SaveChangesAsync();

                return new ResponseModel<cv_res>
                {
                    IsSussess = true,
                    StatusCode = 200,
                    Data = new cv_res
                    {
                        cv_id = cv.ConversationId,
                        cv_name = cv.Title ?? "Không có tên!"
                    }
                };
            }
            return new ResponseModel<cv_res>
            {
                IsSussess = false,
                Message = "conversation early exits"
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

        public async Task<ResponseModel<Conversation_Res>> GetConversation(int userId)
        {
            var conversations = await _context.ConversationMembers.Include(x => x.Conversation).Where(x => x.UserId == userId).ToListAsync();
            var list = new List<Conversation_Res>();
            foreach (var conversation in conversations)
            {
                var media = _mediaGrpc.GetByAssetIdGrpc(new GetByAssetIdRequest { AssetId = conversation.ConversationId.ToString() });
                string json = JsonConvert.SerializeObject(media);
                var response = JsonConvert.DeserializeObject<GrpcResponse>(json);
                if (response.Items.Count > 0 && response.Items != null)
                {
                    list.Add(new Conversation_Res
                    {
                        ConversationId = conversation.ConversationId,
                        ConversationName = conversation?.Title ?? "",
                        PhotoUrl = response?.Items[0].MediaUrl ?? "",
                    });
                }
                else
                {
                    list.Add(new Conversation_Res
                    {
                        ConversationId = conversation.ConversationId,
                        ConversationName = conversation?.Title ?? "",
                    });
                }
            }
            var result = new ResponseModel<Conversation_Res>
            {
                IsSussess = true,
                StatusCode = 200,
                DataList = list
            };
            return result;
        }

        public async Task<List<Message_Res>> GetMessageHistory(Guid conversationId, int userId)
        {
            var messages = await _context.Messages.Where(x => x.ConversationId == conversationId).OrderByDescending(m => m.CreatedAt).ToListAsync();
            var list_data = new List<Message_Res>();
            foreach (var message in messages)
            {
                if (message.SenderId == userId)
                {
                    list_data.Add(new Message_Res
                    {
                        message_id = message.MessageId,
                        side = "right",
                        text = message.Content ?? "",
                    });
                }
                else
                {
                    list_data.Add(new Message_Res
                    {
                        message_id = message.MessageId,
                        side = "left",
                        text = message.Content ?? "",
                    });
                }
            }
            return list_data;

        }

        public Task<ResponseModel<Conversation_Req>> PutConversation(Models.Conversation conversation)
        {
            throw new NotImplementedException();
        }
    }
}
