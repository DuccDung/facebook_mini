using System;
using System.Collections.Generic;

namespace chat_service.Models;

public partial class ConversationMember
{
    public Guid ConversationId { get; set; }

    public int UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool? Remove { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}
