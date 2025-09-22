using System;
using System.Collections.Generic;

namespace profile_service.Models;

public partial class Profile
{
    public Guid ProfileId { get; set; }

    public int UserId { get; set; }

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public byte? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? AvartaUrl { get; set; }

    public string? Address { get; set; }

    public string? Bio { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }
}
