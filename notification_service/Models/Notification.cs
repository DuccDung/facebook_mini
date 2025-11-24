using System;
using System.Collections.Generic;

namespace notification_service.Models;

public partial class Notification
{
    public Guid NotificationId { get; set; }

    public int ReceiverId { get; set; }

    public int ActorId { get; set; }

    public string Type { get; set; } = null!;

    public string AssetId { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
