using Azure;
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
            try
            {
                var conversations = await _context.ConversationMembers.Include(x => x.Conversation).Where(x => x.UserId == userId).ToListAsync();
                var list = new List<Conversation_Res>();
                foreach (var conversation in conversations)
                {
                    var media = _mediaGrpc.GetByAssetIdGrpc(new GetByAssetIdRequest { AssetId = conversation.ConversationId.ToString() });
                    var first = media.Items[0];
                    var createdAt = first.CreateAt.ToDateTime(); 
                    var photoUrl = first.MediaUrl;
                    var isGroup = conversation.Conversation.IsGroup;
                    if (media.Items.Count > 0)
                    {
                        list.Add(new Conversation_Res
                        {
                            ConversationId = conversation.ConversationId,
                            ConversationName = conversation?.Conversation.Title ?? "",
                            PhotoUrl = photoUrl ?? "",
                            IsGroup = isGroup,
                        });
                    }
                    else
                    {
                        list.Add(new Conversation_Res
                        {
                            ConversationId = conversation.ConversationId,
                            ConversationName = conversation?.Title ?? "",
                            IsGroup = isGroup,
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
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new ResponseModel<Conversation_Res>
                {
                    IsSussess = false,
                    StatusCode = 500,
                    Message = "Internal server error"
                };
            }
        }

        public async Task<List<MessageModel>> GetMessageHistory(Guid conversationId, int userId)
        {
            var messages = await _context.Messages.Where(x => x.ConversationId == conversationId).OrderBy(m => m.CreatedAt).ToListAsync();
            var list_data = new List<MessageModel>();
            foreach (var message in messages)
            {
                if (message.SenderId == userId)
                {
                    list_data.Add(new MessageModel
                    {
                        Id = message.MessageId,
                        Side = MessageSide.right,
                        Text = message.Content ?? "",
                        Time = message.CreatedAt
                    });
                }
                else
                {
                    list_data.Add(new MessageModel
                    {
                        Id = message.MessageId,
                        Side = MessageSide.left,
                        Text = message.Content ?? "",
                        Time = message.CreatedAt
                    });
                }
            }
            return list_data;

        }

        public Task<ResponseModel<Conversation_Req>> PutConversation(Models.Conversation conversation)
        {
            throw new NotImplementedException();
        }
        public async Task<string> FormatMessageTime(string utcIsoString)
        {
            if (string.IsNullOrEmpty(utcIsoString))
                return "";

            // Parse chuỗi ISO 8601 (dạng "2025-10-26T16:19:11.1709327Z")
            if (!DateTime.TryParse(utcIsoString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var utcTime))
                return utcIsoString;

            // Chuyển sang giờ địa phương (nếu bạn ở VN)
            var localTime = utcTime.ToLocalTime();
            var now = DateTime.Now;
            var diff = now - localTime;

            // --- Các trường hợp hiển thị ---
            if (localTime.Date == now.Date)
            {
                // Cùng ngày → chỉ hiện giờ:phút
                return localTime.ToString("HH:mm");
            }
            else if (diff.TotalDays < 2)
            {
                return "Hôm qua";
            }
            else if (diff.TotalDays < 7)
            {
                // Trong vòng 7 ngày
                return $"{(int)diff.TotalDays} ngày trước";
            }
            else if (localTime.Year == now.Year)
            {
                // Cùng năm → chỉ hiển thị ngày-tháng
                return localTime.ToString("dd/MM");
            }
            else
            {
                // Khác năm → ngày/tháng/năm
                return localTime.ToString("dd/MM/yyyy");
            }
        }


    }
}
