using System;
using System.Text.Json.Serialization;
using authentication_service.Dtos; 

public sealed class Envelope
{
    [JsonPropertyName("user_id")]
    public int user_id { get; set; } 
    [JsonPropertyName("req")]
    public RegisterRequest Req { get; set; } = new();

    [JsonPropertyName("at")]
    public DateTimeOffset At { get; set; }
}
