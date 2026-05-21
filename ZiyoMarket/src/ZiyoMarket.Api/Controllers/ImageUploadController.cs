using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using ZiyoMarket.Service.DTOs.Auth;

namespace ZiyoMarket.Api.Controllers;

[ApiController]
[Route("api/image_upload")]
[Authorize]
public class ImageUploadController : ControllerBase
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<ImageUploadController> _logger;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int WebpQuality = 82;
    private const int MaxDimension = 1920;

    public ImageUploadController(IOptions<CloudinarySettings> cloudinarySettings, ILogger<ImageUploadController> logger)
    {
        var s = cloudinarySettings.Value;
        var account = new Account(s.CloudName, s.ApiKey, s.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromQuery] string type = "product")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Fayl tanlanmagan" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = "Fayl hajmi 5 MB dan oshmasligi kerak" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { message = $"Ruxsat etilgan formatlar: {string.Join(", ", AllowedExtensions)}" });

        try
        {
            // ImageSharp bilan WebP ga o'tkazish va resize
            using var image = await Image.LoadAsync(file.OpenReadStream());

            if (image.Width > MaxDimension || image.Height > MaxDimension)
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(MaxDimension, MaxDimension),
                    Mode = ResizeMode.Max
                }));

            using var webpStream = new MemoryStream();
            await image.SaveAsync(webpStream, new WebpEncoder { Quality = WebpQuality });
            webpStream.Position = 0;

            // Cloudinary ga yuklash
            var folder = $"ziyo-market/{type}";
            var publicId = $"{folder}/{Guid.NewGuid()}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription("image.webp", webpStream),
                PublicId = publicId,
                Overwrite = false,
                Format = "webp"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary error: {Error}", result.Error.Message);
                return StatusCode(500, new { message = "Rasm yuklashda xatolik yuz berdi" });
            }

            _logger.LogInformation("Cloudinary ga yuklandi: {Url}", result.SecureUrl);

            return Ok(new
            {
                success = true,
                message = "Rasm muvaffaqiyatli yuklandi",
                file_path = result.PublicId,
                full_url = result.SecureUrl.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rasm yuklashda xatolik");
            return StatusCode(500, new { message = "Rasm yuklashda xatolik yuz berdi" });
        }
    }
}
