namespace authentication_service.service
{
    using authentication_service.Models;
    using Microsoft.EntityFrameworkCore;

    public class FriendshipService
    {
        private readonly AuthenticationContext _context;

        public FriendshipService(AuthenticationContext context)
        {
            _context = context;
        }

        // ======================= SEND FRIEND REQUEST =======================
        public async Task<bool> SendFriendRequest(int userId, int friendId)
        {
            // Không gửi cho chính mình
            if (userId == friendId) return false;

            // Kiểm tra ràng buộc đã tồn tại
            var exists = await _context.Friendships
                .AnyAsync(f => (f.UserId == userId && f.FriendId == friendId)
                            || (f.UserId == friendId && f.FriendId == userId));

            if (exists) return false;

            var friendReq = new Friendship
            {
                UserId = userId,
                FriendId = friendId,
                Status = "pending"
            };

            _context.Friendships.Add(friendReq);
            await _context.SaveChangesAsync();
            return true;
        }

        // ======================= ACCEPT FRIEND REQUEST =======================
        public async Task<bool> AcceptFriendRequest(Guid friendshipId)
        {
            var req = await _context.Friendships.FirstOrDefaultAsync(f => f.FriendshipId == friendshipId);

            if (req == null || req.Status != "pending") return false;

            req.Status = "accepted";
            req.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ======================= DECLINE FRIEND REQUEST =======================
        public async Task<bool> DeclineFriendRequest(Guid friendshipId)
        {
            var req = await _context.Friendships.FindAsync(friendshipId);

            if (req == null || req.Status != "pending") return false;

            req.Status = "declined";
            req.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ======================= GET ALL FRIENDS =======================
        public async Task<List<Account>> GetFriends(int userId)
        {
            // userId -> friend
            var friends = await _context.Friendships
                .Where(f =>
                        (f.UserId == userId || f.FriendId == userId)
                        && f.Status == "accept")
                .Select(f => f.UserId == userId ? f.Friend : f.User)
                .ToListAsync();

            return friends;
        }

        // ======================= GET PENDING REQUESTS =======================
        public async Task<List<Friendship>> GetPendingRequests(int userId)
        {
            return await _context.Friendships
                .Where(f => f.FriendId == userId && f.Status == "pending")
                .Include(f => f.User) // Người gửi lời mời
                .ToListAsync();
        }

        public async Task<int> GetMutualFriendsCount(int userId, int friendId)
        {
            // Lấy danh sách bạn của userId
            var userFriends = await _context.Friendships
                .Where(f =>
                    (f.UserId == userId || f.FriendId == userId)
                    && f.Status == "accepted")
                .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                .ToListAsync();

            // Lấy danh sách bạn của friendId
            var friendFriends = await _context.Friendships
                .Where(f =>
                    (f.UserId == friendId || f.FriendId == friendId)
                    && f.Status == "accepted")
                .Select(f => f.UserId == friendId ? f.FriendId : f.UserId)
                .ToListAsync();

            // Tính giao 2 tập hợp
            var mutualCount = userFriends.Intersect(friendFriends).Count();

            return mutualCount;
        }
        public class FInfo
        {
            public bool isFriend { get; set; }
            public string? status { get; set; }
        }

        public async Task<FInfo> isFriend(int userId, int friendId)
        {
            var friendRecord = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == userId)
                );

            if (friendRecord == null)
            {
                // Không tồn tại bất kỳ quan hệ bạn bè nào
                return new FInfo
                {
                    isFriend = false,
                    status = null
                };
            }

            // Nếu tìm thấy record thì trả về info
            return new FInfo
            {
                isFriend = friendRecord.Status == "accept",
                status = friendRecord.Status
            };
        }

    }
}

