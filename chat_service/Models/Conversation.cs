using System;
using System.Collections.Generic;

namespace chat_service.Models;

public class Conversation
{
    public Guid ConversationId { get; set; }

    public bool IsGroup { get; set; }

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool? Remove { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
