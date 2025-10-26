using System.Text.Json;
using System.Text.Json.Serialization;

public class ThreadModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }

    // Bạn đang dùng chuỗi cho thời gian; nếu muốn chuẩn hơn có thể đổi sang DateTimeOffset?
    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = default!;

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("messages")]
    public List<MessageModel>? Messages { get; set; }
}

public class MessageModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("side")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageSide Side { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = default!;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageSide
{
    left,
    right
}
