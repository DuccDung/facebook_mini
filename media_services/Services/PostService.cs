using Azure;
using media_services.Dtos;
using media_services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace media_services.Services
{
    public interface IPostService
    {
        Task<Post> CreatePostAsync(CreatePostRequest req);
        Task<List<PostWithCommentsDto>> GetAllPostsWithCommentsAsync(int userId);
        Task<PostWithCommentsDto?> GetPostWithCommentsAsync(Guid postId);
        Task<PostCommentDto?> CreateCommentAsync(CreateCommentRequest req);
        Task<PostLikeInfoDto> GetPostLikeInfoAsync(Guid postId, int? accountId);

    }

    public class PostService : IPostService
    {
        private readonly MediaContext _context;
        private readonly HttpClient _http;
        private readonly HttpClient _media;

        public PostService(MediaContext context)
        {
            _context = context;
            _http = new HttpClient
            {
                // BaseAddress = new Uri("https://localhost:7070/")
                BaseAddress = new Uri("http://profile_service:8084/")
            };
            _media = new HttpClient
            {
                // BaseAddress = new Uri("https://localhost:7121/")
                BaseAddress = new Uri("http://media_service:8086/")
            };
        }

        public async Task<Post> CreatePostAsync(CreatePostRequest req)
        {
            var post = new Post
            {
                AccountId = req.UserId,
                Content = req.Content,
                PostType = req.PostType,
                CreateAt = DateTime.UtcNow,
                IsRemove = false
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            if (req.TagFriendIds != null && req.TagFriendIds.Count > 0)
            {
                foreach (var friendId in req.TagFriendIds)
                {
                    _context.PostShares.Add(new PostShare
                    {
                        PostId = post.PostId,
                        AccountId = friendId
                    });
                }

                await _context.SaveChangesAsync();
            }

            return post;
        }

        // ============================
        //  Lấy 1 post + comments dạng cây
        // ============================
        public async Task<PostWithCommentsDto?> GetPostWithCommentsAsync(Guid postId)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null) return null;

            // Lấy toàn bộ comments của post (flat list)
            var comments = await _context.PostComments
                .AsNoTracking()
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreateAt)
                .ToListAsync();

            var dto = new PostWithCommentsDto
            {
                PostId = post.PostId,
                AccountId = post.AccountId,
                Content = post.Content,
                PostType = post.PostType,
                CreateAt = post.CreateAt,
                UpdateAt = post.UpdateAt
            };

            dto.Comments = BuildCommentTree(comments , new List<UserProfileDto>());

            return dto;
        }
        public async Task<PostLikeInfoDto> GetPostLikeInfoAsync(Guid postId, int? accountId)
        {
            // Đếm tổng số like của post
            var likeCount = await _context.Likes
                .CountAsync(l => l.AssetId == postId && l.LikeType == 0);

            // Nếu không truyền accountId (chưa login) thì khỏi check
            bool isLiked = false;

            if (accountId.HasValue)
            {
                isLiked = await _context.Likes
                    .AnyAsync(l => l.AssetId == postId
                                && l.AccountId == accountId.Value
                                && l.LikeType == 0);
            }

            return new PostLikeInfoDto
            {
                LikeCount = likeCount,
                IsLiked = isLiked
            };
        }

        // ============================
        // Lấy tất cả post + comments dạng cây
        // ============================
        public async Task<List<PostWithCommentsDto>> GetAllPostsWithCommentsAsync(int userId)
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreateAt)
                .ToListAsync();

            var postIds = posts.Select(p => p.PostId).ToList();

            // Lấy tất cả comments của tất cả post để tránh N+1 query
            var allComments = await _context.PostComments
                .AsNoTracking()
                .Where(c => postIds.Contains(c.PostId))
                .OrderBy(c => c.CreateAt)
                .ToListAsync();

            var result = new List<PostWithCommentsDto>();
            var userIds = posts.Select(p => p.AccountId).Distinct().ToList();
            userIds.AddRange(allComments.Select(c => c.AccountId).Distinct());
            var res = await _http.PostAsJsonAsync("api/Profiles/list", new { UserIds = userIds });
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                Console.WriteLine("Error: " + err);
            }

            var data = await res.Content.ReadFromJsonAsync<List<UserProfileDto>>();
            foreach (var post in posts)
            {
                var commentsOfPost = allComments
                    .Where(c => c.PostId == post.PostId)
                    .ToList();
                var profile = data?.Where(p => p.UserId == post.AccountId).FirstOrDefault();
                var url = $"api/Media/get/by-asset?asset_id={post.PostId.ToString()}";
                var media = await _media.GetFromJsonAsync<List<MediaItemDto>>(url);
                var infoLike = await GetPostLikeInfoAsync(post.PostId , userId);
                var dto = new PostWithCommentsDto
                {
                    PostId = post.PostId,
                    AccountId = post.AccountId,
                    userName = profile != null ? profile.FullName : "Unknown",
                    avatar = profile != null ? profile.BackgroundImgUrl : null,
                    Content = post.Content,
                    PostType = post.PostType,
                    CreateAt = post.CreateAt,
                    UpdateAt = post.UpdateAt,
                    InforLike = infoLike,
                    MediaItems = media ?? new List<MediaItemDto>(),
                    Comments = BuildCommentTree(commentsOfPost , data ?? new List<UserProfileDto>())
                };

                result.Add(dto);
            }

            return result;
        }

        // ============================
        // 📌 Hàm build cây comment từ flat list
        // ============================
        private List<PostCommentDto> BuildCommentTree(List<PostComment> comments , List<UserProfileDto> profiles)
        {
            var dict = new Dictionary<Guid, PostCommentDto>();
            var roots = new List<PostCommentDto>();

            // 1. Convert entity → DTO và đưa vào dictionary
            foreach (var c in comments)
            {
                var profile = profiles?.Where(p => p.UserId == c.AccountId).FirstOrDefault();
                var dto = new PostCommentDto
                {
                    CommentId = c.CommentId,
                    AccountId = c.AccountId,
                    userName = profile != null ? profile.FullName : "Unknown",
                    avatar = profile != null ? profile.BackgroundImgUrl : null,
                    Content = c.Content,
                    ParentCommentId = c.ParentCommentId,
                    CreateAt = c.CreateAt
                };

                dict[c.CommentId] = dto;
            }

            // 2. Gán replies vào parent hoặc đưa vào root list
            foreach (var c in comments)
            {
                var dto = dict[c.CommentId];

                if (c.ParentCommentId.HasValue &&
                    dict.TryGetValue(c.ParentCommentId.Value, out var parentDto))
                {
                    parentDto.Replies.Add(dto);
                }
                else
                {
                    // Comment gốc (không có parent)
                    roots.Add(dto);
                }
            }

            return roots;
        }
        public async Task<PostCommentDto?> CreateCommentAsync(CreateCommentRequest req)
        {
            // 1. Kiểm tra post
            var postExists = await _context.Posts.AnyAsync(p => p.PostId == req.PostId);
            if (!postExists) return null;

            // 2. Nếu là reply → kiểm tra parent có tồn tại
            if (req.ParentCommentId.HasValue)
            {
                var parentExists = await _context.PostComments
                    .AnyAsync(c => c.CommentId == req.ParentCommentId.Value);

                if (!parentExists)
                    throw new Exception("Parent comment not found");
            }
            var url = $"api/Profiles/get-profile?userId={req.AccountId}";
            var profile = await _http.GetFromJsonAsync<ProfileRes>(url);
            // 3. Tạo comment
            var comment = new PostComment
            {
                CommentId = Guid.NewGuid(),
                PostId = req.PostId,
                AccountId = req.AccountId,
                Content = req.Content,
                ParentCommentId = req.ParentCommentId,
                CreateAt = DateTime.UtcNow,
            };

            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            // 4. Mapping sang DTO để trả về FE
            return new PostCommentDto
            {
                CommentId = comment.CommentId,
                AccountId = comment.AccountId,
                userName = profile != null ? profile.FullName : "Unknown",
                avatar = profile != null ? profile.BackgroundImgUrl : null,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                CreateAt = comment.CreateAt,
                Replies = new List<PostCommentDto>() // reply mới tạo chưa có reply con
            };
        }

    }
}
