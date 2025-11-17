
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace authentication_service.Models
{
    public class Friendship
    {
        [Key]
        [Column("friendship_id")]
        public Guid FriendshipId { get; set; } = Guid.NewGuid();

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("friend_id")]
        public int FriendId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===== Navigation Properties =====
        public Account User { get; set; }
        public Account Friend { get; set; }
    }
}

