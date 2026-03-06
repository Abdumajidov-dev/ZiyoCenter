using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Files;
using ZiyoMarket.Service.Enums;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// Unified image upload controller - Professional WebP conversion
/// </summary>
[ApiController]
[Route("api/image_upload")]
public class ImageUploadController : BaseController
{
    private readonly IFileUploadService _fileUploadService;

    public ImageUploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// Upload image with automatic WebP conversion
    /// </summary>
    /// <param name="file">Image file (JPG, PNG, GIF, etc.)</param>
    /// <param name="type">Image type: product, category, banner, user, temp</param>
    /// <returns>Uploaded file details with WebP path</returns>
    /// <remarks>
    /// Example usage:
    ///
    ///     POST /api/image_upload
    ///     Content-Type: multipart/form-data
    ///
    ///     file: [your image file]
    ///     type: product
    ///
    /// Available types:
    /// - product: Product images
    /// - category: Category images
    /// - banner: Banner/promotional images
    /// - user: User avatars
    /// - temp: Temporary uploads
    ///
    /// Features:
    /// - Automatic WebP conversion (smaller file size, better quality)
    /// - Auto-resize if image > 1920x1920 px
    /// - Quality: 80% (optimal balance)
    /// - Max file size: 5MB (before conversion)
    /// - Returns: WebP file path and full URL
    /// </remarks>
    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileUploadResultDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<FileUploadResultDto>> UploadImage(
        [FromForm] IFormFile file,
        [FromQuery] string type)
    {
        try
        {
            // Parse and validate type
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest(new { message = "Image type is required. Available: product, category, banner, user, temp" });
            }

            ImageCategory imageCategory;
            switch (type.ToLowerInvariant())
            {
                case "product":
                    imageCategory = ImageCategory.Product;
                    break;
                case "category":
                    imageCategory = ImageCategory.Category;
                    break;
                case "banner":
                    imageCategory = ImageCategory.Banner;
                    break;
                case "user":
                case "avatar":
                    imageCategory = ImageCategory.User;
                    break;
                case "temp":
                case "temporary":
                    imageCategory = ImageCategory.Temp;
                    break;
                default:
                    return BadRequest(new
                    {
                        message = $"Invalid image type: '{type}'. Available: product, category, banner, user, temp"
                    });
            }

            // Upload and convert to WebP
            var result = await _fileUploadService.UploadImageAsync(file, imageCategory);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload multiple images with automatic WebP conversion
    /// </summary>
    /// <param name="files">List of image files</param>
    /// <param name="type">Image type for all files</param>
    /// <returns>List of uploaded file details</returns>
    [HttpPost("multiple")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(List<FileUploadResultDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<List<FileUploadResultDto>>> UploadMultipleImages(
        [FromForm] List<IFormFile> files,
        [FromQuery] string type)
    {
        try
        {
            // Parse type
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest(new { message = "Image type is required" });
            }

            ImageCategory imageCategory;
            switch (type.ToLowerInvariant())
            {
                case "product":
                    imageCategory = ImageCategory.Product;
                    break;
                case "category":
                    imageCategory = ImageCategory.Category;
                    break;
                case "banner":
                    imageCategory = ImageCategory.Banner;
                    break;
                case "user":
                case "avatar":
                    imageCategory = ImageCategory.User;
                    break;
                case "temp":
                case "temporary":
                    imageCategory = ImageCategory.Temp;
                    break;
                default:
                    return BadRequest(new
                    {
                        message = $"Invalid image type: '{type}'"
                    });
            }

            // Upload all files
            var results = await _fileUploadService.UploadImagesAsync(files, imageCategory);

            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an uploaded image
    /// </summary>
    /// <param name="filePath">Relative file path (e.g., "images/products/abc123.webp")</param>
    /// <returns>Success status</returns>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> DeleteImage([FromQuery] string filePath)
    {
        try
        {
            var deleted = await _fileUploadService.DeleteImageAsync(filePath);

            if (deleted)
                return Ok(new { message = "Image deleted successfully", file_path = filePath });

            return NotFound(new { message = "Image not found", file_path = filePath });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get full URL for a file path
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>Full URL</returns>
    [HttpGet("url")]
    [ProducesResponseType(200)]
    public ActionResult<object> GetFileUrl([FromQuery] string filePath)
    {
        try
        {
            var url = _fileUploadService.GetFileUrl(filePath);
            return Ok(new
            {
                file_path = filePath,
                file_url = url
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
