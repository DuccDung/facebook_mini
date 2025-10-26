namespace media_services.Models
{
    public sealed class B2Options
    {
        public string BucketName { get; set; } = string.Empty;
        public string Bucket { get; set; } = default!;
        public string ServiceURL { get; set; } = default!;
        public bool ForcePathStyle { get; set; } = true;
        public int PresignExpiryMinutes { get; set; } = 15;
        public string? PublicDownloadBase { get; set; }
        public long MaxUploadBytes { get; set; } = 500 * 1024 * 1024;
        public string[] AllowedContentTypes { get; set; } = new[] { "image/", "video/" };
    }

}
