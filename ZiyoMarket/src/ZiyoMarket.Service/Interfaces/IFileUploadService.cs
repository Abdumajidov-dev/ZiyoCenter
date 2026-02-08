using Microsoft.AspNetCore.Http;
using ZiyoMarket.Service.DTOs.Files;
using ZiyoMarket.Service.Enums;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Service for handling file uploads (images, documents, etc.)
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload a single image file
    /// </summary>
    /// <param name="file">The uploaded file</param>
    /// <param name="category">Image category (Product, Category, Banner, User)</param>
    /// <returns>File upload result with URL</returns>
    Task<FileUploadResultDto> UploadImageAsync(IFormFile file, ImageCategory category);

    /// <summary>
    /// Upload multiple image files
    /// </summary>
    /// <param name="files">List of uploaded files</param>
    /// <param name="category">Image category</param>
    /// <returns>List of file upload results</returns>
    Task<List<FileUploadResultDto>> UploadImagesAsync(List<IFormFile> files, ImageCategory category);

    /// <summary>
    /// Delete an image file
    /// </summary>
    /// <param name="filePath">Relative file path (e.g., "images/products/abc123.jpg")</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteImageAsync(string filePath);

    /// <summary>
    /// Delete multiple image files
    /// </summary>
    /// <param name="filePaths">List of relative file paths</param>
    /// <returns>Number of files deleted</returns>
    Task<int> DeleteImagesAsync(List<string> filePaths);

    /// <summary>
    /// Validate image file (size, extension, content type)
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <returns>Validation error message (null if valid)</returns>
    string? ValidateImage(IFormFile file);

    /// <summary>
    /// Get full URL for a file path
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>Full URL to access the file</returns>
    string GetFileUrl(string filePath);
}
