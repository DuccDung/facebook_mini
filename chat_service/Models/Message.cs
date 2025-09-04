using System;
using System.Collections.Generic;

namespace chat_service.Models;

public partial class Message
{
    public Guid MessageId { get; set; }

    public Guid ConversationId { get; set; }

    public int SenderId { get; set; }

    public Guid? LikeId { get; set; }

    public string? Content { get; set; }

    public byte MessageType { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? ParentMessageId { get; set; }

    public bool IsRead { get; set; }

    public bool IsRemove { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
}
