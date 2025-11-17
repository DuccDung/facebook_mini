
using media_services.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace media_services.Contracts
{
    public sealed record UploadOutcome(
        string ObjectKey,
        string ContentType,
        long Size,
        string? ETag,
        string? PublicUrl,
        string? SignedGetUrl
    );

    public interface IMediaService
    {
        Task<UploadOutcome> UploadAsync(IFormFile file, string? folder, CancellationToken ct = default);
        Task<IReadOnlyList<UploadOutcome>> UploadManyAsync(IEnumerable<IFormFile> files, string? folder, CancellationToken ct = default);
        Task DeleteAsync(string objectKey, CancellationToken ct = default);
        Task<List<Medium>> GetByAssetIdAsync(string asset_id);
        Task<List<Medium>> GetImageDemoByAssetIdAsync(string asset_id);
    }
}

