using media_services.Data.MLDb;
using media_services.Dtos;
using media_services.Models;
using media_services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace media_services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly MediaContext _context;
        private readonly LogDbContext _mlcontext;
        private readonly IPostService _postService;
        public PostsController(MediaContext context, LogDbContext mlcontext, IPostService postService)
        {
            _context = context;
            _mlcontext = mlcontext;
            _postService = postService;
        }
        [HttpPost("like")]
        public async Task<IActionResult> LikePost([FromBody] LikeRequest req)
        {
            // đã like rồi → không tạo lại
            var existed = await _context.Likes
                .FirstOrDefaultAsync(x => x.AssetId == req.PostId
                                       && x.AccountId == req.AccountId
                                       && x.LikeType == 0); // 0 = like post

            if (existed != null)
            {
                return Ok(new
                {
                    message = "already_liked",
                    likeCount = await _context.Likes.CountAsync(x => x.AssetId == req.PostId && x.LikeType == 0)
                });
            }

            var like = new Like
            {
                LikeId = Guid.NewGuid(),
                AssetId = req.PostId,
                AccountId = req.AccountId,
                LikeType = 0,
                CreateAt = DateTime.UtcNow
            };
            _mlcontext.Likes.Add(new Models.MLDb.Like
            {
                LikeId = like.LikeId.ToString(),
                PostId = like.AssetId,
                AccountId = like.AccountId.ToString(),
                LikeType = like.LikeType.ToString(),
                CreateAt = like.CreateAt
            });
            _context.Likes.Add(like);
            try
            {
                await _mlcontext.SaveChangesAsync();

            }catch (Exception ex)
            {
                Console.WriteLine("Error saving to MLDb: " + ex.Message);
                Console.WriteLine(ex.InnerException?.Message);
            }
            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes
                .CountAsync(x => x.AssetId == req.PostId && x.LikeType == 0);

            return Ok(new
            {
                message = "liked",
                likeCount = likeCount
            });
        }
        [HttpDelete("unlike")]
        public async Task<IActionResult> UnlikePost([FromBody] LikeRequest req)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(x => x.AssetId == req.PostId
                                       && x.AccountId == req.AccountId
                                       && x.LikeType == 0);

            if (like == null)
            {
                return Ok(new
                {
                    message = "not_liked",
                    likeCount = await _context.Likes.CountAsync(x => x.AssetId == req.PostId && x.LikeType == 0)
                });
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes
                .CountAsync(x => x.AssetId == req.PostId && x.LikeType == 0);

            return Ok(new
            {
                message = "unliked",
                likeCount = likeCount
            });
        }

        [HttpGet("{postId}/detail")]
        public async Task<IActionResult> GetPostDetail(Guid postId)
        {
            var postDto = await _postService.GetPostWithCommentsAsync(postId);
            if (postDto == null) return NotFound();

            return Ok(postDto);
        }

        // GET: api/Posts/list
        [HttpGet("list")]
        public async Task<IActionResult> GetAllPosts(int userId)
        {
            var posts = await _postService.GetAllPostsWithCommentsAsync(userId);
            return Ok(posts);
        }
        // ============================================
        // API 1: Tạo COMMENT CHA
        // POST: /api/Posts/comment
        // ============================================
        [HttpPost("comment")]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest req)
        {
            if (req.PostId == Guid.Empty || req.AccountId <= 0)
                return BadRequest("Invalid request");

            req.ParentCommentId = null; // ép thành comment cha

            var result = await _postService.CreateCommentAsync(req);
            if (result == null) return NotFound("Post not found");
           
            return Ok(new
            {
                message = "Comment created successfully",
                comment = result
            });
        }

        // ============================================
        // API 2: Tạo COMMENT CON (REPLY)
        // POST: /api/Posts/comment/reply
        // ============================================
        [HttpPost("comment/reply")]
        public async Task<IActionResult> ReplyComment([FromBody] CreateCommentRequest req)
        {
            if (req.PostId == Guid.Empty || req.AccountId <= 0 || req.ParentCommentId == null)
                return BadRequest("Invalid reply request");

            var result = await _postService.CreateCommentAsync(req);
            if (result == null) return NotFound("Post not found");

            return Ok(new
            {
                message = "Reply created successfully",
                reply = result
            });
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest req)
        {
            if (req == null || req.UserId == 0)
                return BadRequest("Invalid request");

            // 1) Tạo bài viết
            var post = new Post
            {
                AccountId = req.UserId,
                Content = req.Content,
                PostType = req.PostType,
                CreateAt = DateTime.UtcNow,
                IsRemove = false
            };
            _mlcontext.Posts.Add(new Models.MLDb.Post
            {
                PostId = post.PostId,
                AccountId = post.AccountId.ToString(),
                Content = post.Content,
                PostType = post.PostType.ToString(),
                CreateAt = post.CreateAt,
                IsRemove = post.IsRemove
            });
           await _mlcontext.SaveChangesAsync();

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();   // lưu để có PostId

            // 2) Nếu có tag bạn bè → insert vào bảng post_shares
            if (req.TagFriendIds != null && req.TagFriendIds.Count > 0)
            {
                foreach (var friendId in req.TagFriendIds)
                {
                    var share = new PostShare
                    {
                        PostId = post.PostId,
                        AccountId = friendId   // người được tag
                    };

                    _context.PostShares.Add(share);
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = "Post created successfully",
                PostId = post.PostId
            });
        }
    }
}
