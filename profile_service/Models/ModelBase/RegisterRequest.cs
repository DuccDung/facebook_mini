using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public enum SexType
{
    Custom = -1,
    Female = 1,
    Male = 2
}

public sealed class BirthdayDto
{
    [JsonPropertyName("day")]
    public int? Day { get; set; }

    [JsonPropertyName("month")]
    public int? Month { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }
}

public sealed class RegisterRequest
{
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; } = string.Empty;

    [JsonPropertyName("lastname")]
    public string Lastname { get; set; } = string.Empty;

    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [EmailAddress]
    [JsonPropertyName("email_confirmation")]
    public string EmailConfirmation { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("birthday")]
    public BirthdayDto? Birthday { get; set; }

    [JsonPropertyName("sex")]
    public SexType? Sex { get; set; }

    [JsonPropertyName("preferred_pronoun")]
    public string PreferredPronoun { get; set; } = string.Empty;

    [JsonPropertyName("custom_gender")]
    public string CustomGender { get; set; } = string.Empty;

    [JsonPropertyName("referrer")]
    public string Referrer { get; set; } = string.Empty;
}

public sealed class Envelope
{
    [JsonPropertyName("user_id")]
    public int user_id { get; set; } 

    [JsonPropertyName("req")]
    public RegisterRequest Req { get; set; } = new();

    // Dùng DateTimeOffset để giữ đúng múi giờ/Zulu trong chuỗi ISO
    [JsonPropertyName("at")]
    public DateTimeOffset At { get; set; }
}
