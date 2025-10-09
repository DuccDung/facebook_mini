using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace media_services.Contracts
{
    // Single upload form
    public sealed class MediaUploadForm
    {
        public IFormFile? File { get; set; }
        public string? Folder { get; set; } = "uploads";
    }

    // Multi upload form
    public sealed class MediaUploadManyForm
    {
        public List<IFormFile> Files { get; set; } = new();
        public string? Folder { get; set; } = "uploads";
    }

    // Delete
    public sealed class DeleteRequest
    {
        public string ObjectKey { get; set; } = default!;
    }
}
