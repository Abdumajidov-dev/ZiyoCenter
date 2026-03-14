using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Files;
using ZiyoMarket.Service.Enums;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers;

/// <summary>
/// File upload management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileUploadController : BaseController
{
    private readonly IFileUploadService _fileUploadService;

    public FileUploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// Upload a product image
    /// </summary>
    /// <param name="file">Image file</param>
    /// <returns>File upload result with URL</returns>
    [HttpPost("product")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUploadResultDto>> UploadProductImage([FromForm] IFormFile file)
    {
        try
        {
            var result = await _fileUploadService.UploadImageAsync(file, ImageCategory.Product);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload multiple product images
    /// </summary>
    /// <param name="files">List of image files</param>
    /// <returns>List of file upload results</returns>
    [HttpPost("product/multiple")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<FileUploadResultDto>>> UploadProductImages([FromForm] List<IFormFile> files)
    {
        try
        {
            var results = await _fileUploadService.UploadImagesAsync(files, ImageCategory.Product);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload a category image
    /// </summary>
    /// <param name="file">Image file</param>
    /// <returns>File upload result with URL</returns>
    [HttpPost("category")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUploadResultDto>> UploadCategoryImage([FromForm] IFormFile file)
    {
        try
        {
            var result = await _fileUploadService.UploadImageAsync(file, ImageCategory.Category);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload a banner image
    /// </summary>
    /// <param name="file">Image file</param>
    /// <returns>File upload result with URL</returns>
    [HttpPost("banner")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUploadResultDto>> UploadBannerImage([FromForm] IFormFile file)
    {
        try
        {
            var result = await _fileUploadService.UploadImageAsync(file, ImageCategory.Banner);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload multiple banner images
    /// </summary>
    /// <param name="files">List of image files</param>
    /// <returns>List of file upload results</returns>
    [HttpPost("banner/multiple")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<FileUploadResultDto>>> UploadBannerImages([FromForm] List<IFormFile> files)
    {
        try
        {
            var results = await _fileUploadService.UploadImagesAsync(files, ImageCategory.Banner);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Upload a user avatar/profile image
    /// </summary>
    /// <param name="file">Image file</param>
    /// <returns>File upload result with URL</returns>
    [HttpPost("user/avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUploadResultDto>> UploadUserAvatar([FromForm] IFormFile file)
    {
        try
        {
            var result = await _fileUploadService.UploadImageAsync(file, ImageCategory.User);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an image file
    /// </summary>
    /// <param name="filePath">Relative file path (e.g., "images/products/abc123.jpg")</param>
    /// <returns>Success status</returns>
    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteImage([FromQuery] string filePath)
    {
        try
        {
            var deleted = await _fileUploadService.DeleteImageAsync(filePath);

            if (deleted)
                return Ok(new { message = "Image deleted successfully" });

            return NotFound(new { message = "Image not found" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete multiple image files
    /// </summary>
    /// <param name="filePaths">List of relative file paths</param>
    /// <returns>Number of files deleted</returns>
    [HttpPost("delete-multiple")]
    [Authorize]
    public async Task<ActionResult> DeleteImages([FromBody] List<string> filePaths)
    {
        try
        {
            var deletedCount = await _fileUploadService.DeleteImagesAsync(filePaths);
            return Ok(new { deleted_count = deletedCount, message = $"{deletedCount} images deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get file URL for a given path
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>Full URL to access the file</returns>
    [HttpGet("url")]
    public ActionResult<string> GetFileUrl([FromQuery] string filePath)
    {
        try
        {
            var url = _fileUploadService.GetFileUrl(filePath);
            return Ok(new { file_url = url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
