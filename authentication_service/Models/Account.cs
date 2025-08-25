using System;
using System.Collections.Generic;

namespace authentication_service.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string AccountName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhotoUrl { get; set; }
}
