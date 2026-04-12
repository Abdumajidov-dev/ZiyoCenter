using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Rasm yuklash controller
/// </summary>
[ApiController]
[Route("api/image_upload")]
[Authorize]
public class ImageUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageUploadController> _logger;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public ImageUploadController(IWebHostEnvironment env, ILogger<ImageUploadController> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Rasm yuklash (mahsulot yoki kategoriya)
    /// </summary>
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
            var year = DateTime.UtcNow.Year.ToString();
            var month = DateTime.UtcNow.Month.ToString("00");
            
            // wwwroot/uploads/{type}/{year}/{month}/ papkasiga saqlash
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", type, year, month);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Relative path qaytarish
            var relativePath = $"uploads/{type}/{year}/{month}/{fileName}";

            _logger.LogInformation("Rasm yuklandi: {Path}", relativePath);

            return Ok(new
            {
                success = true,
                message = "Rasm muvaffaqiyatli yuklandi",
                file_path = relativePath,
                full_url = $"{Request.Scheme}://{Request.Host}/{relativePath}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rasm yuklashda xatolik");
            return StatusCode(500, new { message = "Rasm yuklashda xatolik yuz berdi" });
        }
    }
}
