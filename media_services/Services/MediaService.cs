using media_services.Contracts;
using media_services.Interface;      // <- IObjectStorage của bạn
using media_services.Models;        // <- B2Options của bạn
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace media_services.Services
{
    public sealed class MediaService : IMediaService
    {
        private readonly IObjectStorage _storage;
        private readonly B2Options _opt;
        private readonly MediaContext _context;
        private readonly IAmazonS3 _s3;
        public MediaService(IObjectStorage storage, IOptions<B2Options> opt, MediaContext context, IAmazonS3 s3)
        {
            _storage = storage;
            _opt = opt.Value;
            _context = context;
            _s3 = s3;
        }

        public async Task<UploadOutcome> UploadAsync(IFormFile file, string? folder, CancellationToken ct = default)
        {
            ValidateFile(file);

            await using var stream = file.OpenReadStream();
            var r = await _storage.UploadAsync(
                stream,
                file.FileName,
                file.ContentType ?? "application/octet-stream",
                folder ?? "uploads",
                ct);

            return new UploadOutcome(r.ObjectKey, r.ContentType, r.Size, r.ETag, r.PublicUrl, r.SignedGetUrl);
        }

        public async Task<IReadOnlyList<UploadOutcome>> UploadManyAsync(IEnumerable<IFormFile> files, string? folder, CancellationToken ct = default)
        {
            var list = files?.ToList() ?? [];
            if (list.Count == 0) throw new ArgumentException("No files provided.", nameof(files));

            // kiểm tra tổng dung lượng (tuỳ chọn)
            long total = list.Sum(f => f?.Length ?? 0);
            if (total == 0) throw new ArgumentException("All files are empty.", nameof(files));
            if (total > Math.Max(_opt.MaxUploadBytes, _opt.MaxUploadBytes * list.Count))
            {
                // bạn có thể đặt rule riêng; ở đây chỉ minh hoạ
            }

            var results = new List<UploadOutcome>(list.Count);
            foreach (var f in list)
            {
                if (f is null || f.Length == 0) continue;
                ValidateFile(f);

                await using var stream = f.OpenReadStream();
                var r = await _storage.UploadAsync(
                    stream,
                    f.FileName,
                    f.ContentType ?? "application/octet-stream",
                    folder ?? "uploads",
                    ct);

                results.Add(new UploadOutcome(r.ObjectKey, r.ContentType, r.Size, r.ETag, r.PublicUrl, r.SignedGetUrl));
            }

            return results;
        }

        public Task DeleteAsync(string objectKey, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
                throw new ArgumentException("objectKey is required.", nameof(objectKey));

            return _storage.DeleteAsync(objectKey, ct);
        }

        private void ValidateFile(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Missing file.");

            if (file.Length > _opt.MaxUploadBytes)
                throw new InvalidOperationException($"File too large. Max = {_opt.MaxUploadBytes} bytes.");

            var ctType = file.ContentType ?? "application/octet-stream";
            if (!_opt.AllowedContentTypes.Any(p => ctType.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Content-Type not allowed: {ctType}");
        }
        //public async Task<string> GetSignedUrlAsync(string objectKey)
        //{
        //    var req = new GetPreSignedUrlRequest
        //    {
        //        BucketName = "socialnetworkfacebook",
        //        Key = objectKey,
        //        Expires = DateTime.UtcNow.AddMinutes(30),
        //        Verb = HttpVerb.GET
        //    };
        //    return _s3.GetPreSignedURL(req);
        //}


        public async Task<List<Medium>> GetByAssetIdAsync(string asset_id)
        {
            try
            {
                var res = await _context.Media.Where(x => x.AssetId == asset_id).ToListAsync();
                foreach (var item in res)
                {
                    item.MediaUrl = "http://13.112.144.107:5006"+ item.MediaUrl;
                }
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Medium>();
            }
        }

        public async Task<List<Medium>> GetImageDemoByAssetIdAsync(string asset_id)
        {
            try
            {
                var res = await _context.Media
                    .Where(x => x.AssetId == asset_id)
                    .OrderByDescending(x => x.CreateAt)
                    .Take(10)
                    .ToListAsync();

                foreach (var item in res)
                {
                    item.MediaUrl = "http://13.112.144.107:5006"+ item.MediaUrl;
                }

                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Medium>();
            }
        }

    }
}
