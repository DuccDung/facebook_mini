using System.Text.Json.Serialization;

namespace media_services.Dtos
{
    public class MediaItemDto
    {
        [JsonPropertyName("mediaId")]
        public Guid MediaId { get; set; }

        [JsonPropertyName("mediaUrl")]
        public string MediaUrl { get; set; }

        [JsonPropertyName("mediaType")]
        public string MediaType { get; set; }

        [JsonPropertyName("createAt")]
        public DateTime CreateAt { get; set; }

        [JsonPropertyName("objectKey")]
        public string ObjectKey { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("assetId")]
        public string AssetId { get; set; }
    }
}
