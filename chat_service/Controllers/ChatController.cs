using AuthorizationProto;
using chat_service.Internal;
using chat_service.Models.ModelBase;
using MediaProto;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace chat_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AuthorizationGrpcService.AuthorizationGrpcServiceClient _authzGrpc;
        private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpc;
        private readonly IConversation _conversation;

        public ChatController(AuthorizationGrpcService.AuthorizationGrpcServiceClient authzGrpc, MediaGrpcService.MediaGrpcServiceClient mediaGrpc, IConversation conversation)
        {
            _authzGrpc = authzGrpc;
            _mediaGrpc = mediaGrpc;
            _conversation = conversation;
        }

        [HttpGet]
        [Route("test")]
        public IActionResult Tesst()
        {
            var request = new GetByAssetIdRequest
            {
                AssetId = "xxx"
            };
            var x = _mediaGrpc.GetByAssetIdGrpc(request);
            return Ok(x);
        }
        // GET: api/chat/permissions?userId=1&assetId=10
        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions([FromQuery] string userId, [FromQuery] string assetId)
        {
            var request = new GetPermissionsRequest
            {
                UserId = userId,
                AssetId = assetId
            };

            var reply = await _authzGrpc.GetPermissionsAsync(request);

            return Ok(new
            {
                UserId = userId,
                AssetId = assetId,
                Permissions = reply.Permissions.ToArray()
            });
        }

        [HttpPost]
        [Route("InitConversation")]
        public async Task<IActionResult> InitConversation(Conversation_Req req)
        {
            var res = await _conversation.CreateConversation1v1(req);
            if (!res.IsSussess) return BadRequest();
            return Ok(res);
        }
        [HttpGet]
        [Route("GetConversation")]
        public async Task<IActionResult> GetConversation(int userId)
        {
            var consersations = await _conversation.GetConversation(userId);
            List<ThreadModel> threadModels = new List<ThreadModel>();
            if (!consersations.IsSussess) return BadRequest();
            foreach (var cv in consersations.DataList ?? new List<Conversation_Res>())
            {
                var messages =await _conversation.GetMessageHistory(cv.ConversationId, userId);
                ThreadModel thread = new ThreadModel
                {
                    Id = cv.ConversationId,
                    Name = cv.ConversationName,
                    Avatar = cv.PhotoUrl ?? "https://example.com/default-avatar.png",
                    Snippet = messages.FirstOrDefault()?.Text,
                    Time = await _conversation.FormatMessageTime(DateTime.UtcNow.ToString("o")),
                    Active = false,
                    Messages = messages.Select(m => new MessageModel
                    {
                        Id = m.Id,
                        Side = m.Side,
                        Text = m.Text
                    }).ToList()
                };
                threadModels.Add(thread);
            }
            // fix tới đây
            return Ok(threadModels);
          //  return Ok(consersations);
        }
    }


}
