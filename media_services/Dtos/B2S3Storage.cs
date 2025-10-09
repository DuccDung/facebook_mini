namespace media_services.Models
{
    using Amazon.Runtime;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Microsoft.Extensions.Options;
    using media_services.Interface;

    public sealed class B2S3Storage : IObjectStorage
    {
        private readonly IAmazonS3 _s3;
        private readonly B2Options _opt;

        public B2S3Storage(IAmazonS3 s3, IOptions<B2Options> options)
        {
            _s3 = s3;
            _opt = options.Value;
        }

        public async Task<MediaUploadResult> UploadAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct = default)
        {
            var safeName = SanitizeFileName(fileName);
            var key = $"{(string.IsNullOrWhiteSpace(folder) ? "uploads" : folder)}/{DateTimeOffset.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{safeName}";

            var put = new PutObjectRequest
            {
                BucketName = _opt.Bucket,
                Key = key,
                InputStream = stream,
                AutoCloseStream = false,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
            };

            var resp = await _s3.PutObjectAsync(put, ct);

            string? publicUrl = _opt.PublicDownloadBase is null
                ? null
                : $"{_opt.PublicDownloadBase}/{_opt.Bucket}/{Uri.EscapeDataString(key)}";

            var getReq = new GetPreSignedUrlRequest
            {
                BucketName = _opt.Bucket,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(_opt.PresignExpiryMinutes)
            };
            var signedGet = _s3.GetPreSignedURL(getReq);

            return new MediaUploadResult(
                key, publicUrl, signedGet, put.ContentType, stream.CanSeek ? stream.Length : 0, resp.ETag
            );
        }

        public Task DeleteAsync(string objectKey, CancellationToken ct = default)
            => _s3.DeleteObjectAsync(new DeleteObjectRequest { BucketName = _opt.Bucket, Key = objectKey }, ct);

        private static string SanitizeFileName(string name)
        {
            var bad = Path.GetInvalidFileNameChars();
            var clean = new string(name.Select(ch => bad.Contains(ch) ? '-' : ch).ToArray());
            return clean.Trim();
        }
    }

}
