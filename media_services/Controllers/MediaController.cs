using media_services.Contracts;
using media_services.Interface;
using media_services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace media_services.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public sealed class MediaController : ControllerBase
    {
        private readonly IMediaService _svc;
        private readonly MediaContext _context;
        public MediaController(IMediaService svc , MediaContext context)
        {
            _svc = svc;
            _context = context;
        }

        // test
        //[HttpGet]
        //public async Task<IActionResult> tesst()
        //{
        //    try
        //    {
        //        var test = await _context.Media.Where(x => x.AssetId== "xxx").ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    var tesst = await _svc.GetByAssetIdAsync("xxx");
        //    return Ok(tesst);
        //}
        [HttpGet]
        [Route("get/by-asset")]
        public async Task<IActionResult> GetByAssetId(string asset_id, CancellationToken ct)
        {
            var res = await _svc.GetByAssetIdAsync(asset_id);
            return Ok(res);
        }
        // --- Single file ---
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Route("upload/file")]
        public async Task<IActionResult> Upload(string asset_id, [FromForm] MediaUploadForm form, CancellationToken ct)
        {
            if (form.File is null) return BadRequest("Missing file");
            var r = await _svc.UploadAsync(form.File, form.Folder, ct);
            if (r == null) return BadRequest(r);
            var res = new Medium
            {
                AssetId = asset_id,
                CreateAt = DateTime.Now,
                MediaType = r.ContentType,
                MediaUrl = r.SignedGetUrl,
                ObjectKey = r.ObjectKey,
                Size = r.Size,
            };
            await _context.Media.AddAsync(res);
            await _context.SaveChangesAsync();
            return Ok(r);
        }

        // --- Multi files ---
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Route("upload/files")]
        public async Task<IActionResult> UploadMany(string asset_id, [FromForm] MediaUploadManyForm form, CancellationToken ct)
        {
            if (form.Files is null || form.Files.Count == 0) return BadRequest("No files");
            var r = await _svc.UploadManyAsync(form.Files, form.Folder, ct);
            if (r == null) return BadRequest();
            var res = new List<Medium>();
            foreach (var file in r)
            {
                res.Add(new Medium
                {
                    AssetId = asset_id,
                    CreateAt = DateTime.Now,
                    MediaType = file.ContentType,
                    MediaUrl = file.SignedGetUrl,
                    ObjectKey = file.ObjectKey,
                    Size = file.Size,
                });
            }

            await _context.Media.AddRangeAsync(res);
            await _context.SaveChangesAsync();
            return Ok(r);
        }

        [HttpDelete]
        [Route("delete/file")]
        public async Task<IActionResult> Delete(string media_id, [FromBody] DeleteRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.ObjectKey)) return BadRequest("objectKey required");
            await _svc.DeleteAsync(req.ObjectKey, ct);
            var media = await _context.Media.FindAsync(media_id);
            if (media == null) return BadRequest(media);
            _context.Media.Remove(media);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
