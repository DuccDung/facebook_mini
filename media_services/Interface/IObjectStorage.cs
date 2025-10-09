namespace media_services.Interface
{
    public interface IObjectStorage
    {
        Task<MediaUploadResult> UploadAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct = default);
        Task DeleteAsync(string objectKey, CancellationToken ct = default);
    }

    public sealed record MediaUploadResult(
        string ObjectKey,
        string? PublicUrl,
        string SignedGetUrl,
        string ContentType,
        long Size,
        string? ETag
    );

}
