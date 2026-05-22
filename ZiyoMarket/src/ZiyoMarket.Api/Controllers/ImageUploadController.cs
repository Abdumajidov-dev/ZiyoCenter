using Amazon.S3;
using Amazon.S3.Model;
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
    private readonly S3Settings _s3;
    private readonly ILogger<ImageUploadController> _logger;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int WebpQuality = 82;
    private const int MaxDimension = 1920;

    public ImageUploadController(IOptions<S3Settings> s3Settings, ILogger<ImageUploadController> logger)
    {
        _s3 = s3Settings.Value;
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
            // WebP ga o'tkazish va resize
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

            // S3 ga yuklash
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("00");
            var fileName = $"{Guid.NewGuid()}.webp";
            var key = $"{type}/{year}/{month}/{fileName}";

            using var s3Client = BuildS3Client();

            var putRequest = new PutObjectRequest
            {
                BucketName = _s3.BucketName,
                Key = key,
                InputStream = webpStream,
                ContentType = "image/webp",
                CannedACL = S3CannedACL.PublicRead
            };

            await s3Client.PutObjectAsync(putRequest);

            var fullUrl = $"{_s3.PublicUrl.TrimEnd('/')}/{key}";

            _logger.LogInformation("S3 ga yuklandi: {Url}", fullUrl);

            return Ok(new
            {
                success = true,
                message = "Rasm muvaffaqiyatli yuklandi",
                file_path = key,
                full_url = fullUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rasm yuklashda xatolik");
            return StatusCode(500, new { message = "Rasm yuklashda xatolik yuz berdi", detail = ex.Message });
        }
    }

    private AmazonS3Client BuildS3Client()
    {
        var config = new AmazonS3Config { ForcePathStyle = true };

        if (!string.IsNullOrWhiteSpace(_s3.ServiceUrl))
            config.ServiceURL = _s3.ServiceUrl;

        return new AmazonS3Client(_s3.AccessKey, _s3.SecretKey, config);
    }
}
