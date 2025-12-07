using media_services.Contracts;
using media_services.Interface;
using media_services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Hosting;
namespace media_services.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public sealed class MediaController : ControllerBase
    {
        private readonly IMediaService _svc;
        private readonly MediaContext _context;
        public MediaController(IMediaService svc, MediaContext context)
        {
            _svc = svc;
            _context = context;
        }
        //[HttpGet]
        //[Route("initialize-img-cv")] // https://localhost:7121/api/Media/initialize-img-cv?profileId=x&cv_id=x
        //public async Task<IActionResult> InitializeImgCv(string profileId, string cv_id)
        //{
        //    var avatar = await _context.Media.Where(m => m.AssetId == profileId && m.MediaType == "background_image").FirstOrDefaultAsync();
        //    if (avatar == null) return BadRequest("not found data media!");
        //    var media = new Medium
        //    {
        //        AssetId = cv_id,
        //        CreateAt = DateTime.Now,
        //        MediaType = "conversation_image",
        //        MediaUrl = avatar.MediaUrl,
        //        ObjectKey = avatar.ObjectKey,
        //        Size = avatar.Size,
        //    };
        //    await _context.Media.AddAsync(media);
        //    await _context.SaveChangesAsync();
        //    return Ok("initialize image success!");
        //}
        [HttpGet]
        [Route("get/by-asset")]
        public async Task<IActionResult> GetByAssetId(string asset_id, CancellationToken ct)
        {
            var res = await _svc.GetByAssetIdAsync(asset_id);
            return Ok(res);
        }
        [HttpGet]
        [Route("get/images-demo")]
        public async Task<IActionResult> GetImgDemoByAssetId(string asset_id, CancellationToken ct)
        {
            var res = await _svc.GetImageDemoByAssetIdAsync(asset_id);
            return Ok(res);
        }
        [HttpGet]
        [Route("get/update-bacground-img")]
        public async Task<IActionResult> UploadBacgroundImg(string mediaId, string assetId, CancellationToken ct)
        {
            var bgImgOld = await _context.Media.Where(m => m.MediaType == "background_image" && m.AssetId == assetId).ToListAsync();
            if (bgImgOld != null)
            {
                foreach (var bgImgOldItem in bgImgOld) bgImgOldItem.MediaType = "image/jpeg";
                await _context.SaveChangesAsync();
            }
            var media = await _context.Media.FirstOrDefaultAsync(m => m.MediaId.ToString() == mediaId);
            if (media == null) return BadRequest("media not found");
            media.MediaType = "background_image";
            _context.Media.Update(media);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Route("upload/img")]
        public async Task<IActionResult> Upload(string asset_id, string asset_type, [FromForm] MediaUploadForm form, CancellationToken ct)
        {
            if (form.File is null) return BadRequest("Missing file");

            // Đường dẫn đến thư mục wwwroot
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // Tạo thư mục con theo asset_id (nếu chưa tồn tại)
            var assetFolder = Path.Combine(webRootPath, asset_id);
            if (!Directory.Exists(assetFolder))
            {
                Directory.CreateDirectory(assetFolder);
            }

            // Lưu tệp vào thư mục wwwroot/[asset_id]/ 
            var filePath = Path.Combine(assetFolder, form.File.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await form.File.CopyToAsync(fileStream);
            }

            // Lưu thông tin về media vào cơ sở dữ liệu
            var res = new Medium
            {
                AssetId = asset_id,
                CreateAt = DateTime.Now,
                MediaType = asset_type,
                MediaUrl = Path.Combine("/" + asset_id, form.File.FileName),  // Đường dẫn tương đối từ wwwroot
                ObjectKey = form.File.FileName,  // Lưu tên file hoặc một khóa duy nhất khác tùy theo yêu cầu của bạn
                Size = form.File.Length,
            };

            // Lưu vào cơ sở dữ liệu
            await _context.Media.AddAsync(res);
            await _context.SaveChangesAsync();

            return Ok(res);
        }

        // --- Single file ---
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Route("upload/file")]
        public async Task<IActionResult> Upload(string asset_id, [FromForm] MediaUploadForm form, CancellationToken ct)
        {
            if (form.File is null) return BadRequest("Missing file");

            // Lấy đường dẫn đến thư mục wwwroot trên server
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // Tạo thư mục con theo asset_id nếu chưa có
            var assetFolder = Path.Combine(webRootPath, asset_id);
            if (!Directory.Exists(assetFolder))
            {
                Directory.CreateDirectory(assetFolder);
            }

            // Đặt tên file khi lưu trữ
            var fileName = form.File.FileName;
            var filePath = Path.Combine(assetFolder, fileName);

            // Lưu file vào thư mục wwwroot/[asset_id]
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await form.File.CopyToAsync(fileStream, ct);
            }

            // Tạo đối tượng Media để lưu vào cơ sở dữ liệu
            var res = new Medium
            {
                AssetId = asset_id,
                CreateAt = DateTime.Now,
                MediaType = form.File.ContentType,
                MediaUrl = $"/{asset_id}/{fileName}",  // Đường dẫn tương đối từ thư mục wwwroot
                ObjectKey = fileName,  // Lưu tên file hoặc một khóa duy nhất khác nếu cần
                Size = form.File.Length,
            };

            // Lưu vào cơ sở dữ liệu
            await _context.Media.AddAsync(res);
            await _context.SaveChangesAsync();

            return Ok(res);
        }


        // --- Multi files ---
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Route("upload/files")]
        public async Task<IActionResult> UploadMany(string asset_id, [FromForm] MediaUploadManyForm form, CancellationToken ct)
        {
            if (form.Files is null || form.Files.Count == 0) return BadRequest("No files");

            // Lấy đường dẫn đến thư mục wwwroot trên server
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // Tạo thư mục con theo asset_id nếu chưa có
            var assetFolder = Path.Combine(webRootPath, asset_id);
            if (!Directory.Exists(assetFolder))
            {
                Directory.CreateDirectory(assetFolder);
            }

            var res = new List<Medium>();
            foreach (var file in form.Files)
            {
                // Đặt tên file khi lưu trữ
                var fileName = file.FileName;
                var filePath = Path.Combine(assetFolder, fileName);

                // Lưu file vào thư mục wwwroot/[asset_id]
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream, ct);
                }

                // Tạo đối tượng Media để lưu vào cơ sở dữ liệu
                res.Add(new Medium
                {
                    AssetId = asset_id,
                    CreateAt = DateTime.Now,
                    MediaType = file.ContentType,
                    MediaUrl = $"/{asset_id}/{fileName}",  // Đường dẫn tương đối từ thư mục wwwroot
                    ObjectKey = fileName,  // Lưu tên file hoặc một khóa duy nhất khác nếu cần
                    Size = file.Length,
                });
            }

            // Lưu vào cơ sở dữ liệu
            await _context.Media.AddRangeAsync(res);
            await _context.SaveChangesAsync();

            return Ok(res);
        }

        [HttpDelete]
        [Route("delete/file")]
        public async Task<IActionResult> Delete(string media_id, [FromBody] DeleteRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.ObjectKey)) return BadRequest("objectKey required");

            // Tìm media trong cơ sở dữ liệu
            var media = await _context.Media.FindAsync(media_id);
            if (media == null) return BadRequest("Media not found");

            // Xóa tệp tin khỏi hệ thống tệp (wwwroot)
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, media.AssetId, media.ObjectKey); // Đảm bảo rằng media.ObjectKey là tên tệp

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath); // Xóa tệp tin khỏi hệ thống
            }
            else
            {
                return BadRequest("File not found on server.");
            }

            // Xóa media khỏi cơ sở dữ liệu
            _context.Media.Remove(media);
            await _context.SaveChangesAsync();

            return NoContent(); // Trả về HTTP 204 No Content nếu xóa thành công
        }

    }
}
