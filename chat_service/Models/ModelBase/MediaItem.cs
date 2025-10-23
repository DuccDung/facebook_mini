namespace chat_service.Models.ModelBase
{
    public class MediaItem
    {
        public string MediaId { get; set; }
        public string AssetId { get; set; }
        public string MediaUrl { get; set; }
        public string MediaType { get; set; }
        public DateTime CreateAt { get; set; }
        public long Size { get; set; }
        public string ObjectKey { get; set; }
    }

    public class GrpcResponse
    {
        public List<MediaItem> Items { get; set; }
    }

}
