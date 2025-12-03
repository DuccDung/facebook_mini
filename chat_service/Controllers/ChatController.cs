using AuthorizationProto;
using chat_service.Internal;
using chat_service.Models.ModelBase;
using MediaProto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace chat_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AuthorizationGrpcService.AuthorizationGrpcServiceClient _authzGrpc;
        private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpc;
        private readonly IConversation _conversation;
        private readonly HttpClient _media;
        private readonly HttpClient _profile;
        public ChatController(AuthorizationGrpcService.AuthorizationGrpcServiceClient authzGrpc, MediaGrpcService.MediaGrpcServiceClient mediaGrpc, IConversation conversation)
        {
            _authzGrpc = authzGrpc;
            _mediaGrpc = mediaGrpc;
            _conversation = conversation;
            _media = new HttpClient
            {
                // BaseAddress = new Uri("https://localhost:7121/")
                BaseAddress = new Uri("http://media_service:8086/")
            };
            _profile = new HttpClient
            {
                //  BaseAddress = new Uri("https://localhost:7070/")
                BaseAddress = new Uri("http://profile_service:8084/")
            };
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
        [HttpPost]
        [Route("InitConversationGroup")]
        public async Task<IActionResult> InitConversationGroup(ConversationList_Req req)
        {
            var res = await _conversation.CreateConversationGroup(req);
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
                //var json = JsonConvert.SerializeObject(cv.PhotoUrl);
                ThreadModel thread = new ThreadModel
                {
                    Id = cv.ConversationId,
                    Name = cv.ConversationName,
                    Avatar = JsonConvert.SerializeObject(cv.PhotoUrl).ToString() ?? "https://example.com/default-avatar.png",
                    Snippet = messages.FirstOrDefault()?.Text,
                    Time = await _conversation.FormatMessageTime(DateTime.UtcNow.ToString("o")),
                    Active = false,
                    IsGroup = cv.IsGroup,
                    Messages = messages.Select(m => new MessageModel
                    {
                        Id = m.Id,
                        Side = m.Side,
                        Text = m.Text,
                        Time = m.Time,
                        Name = cv.ConversationName
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
