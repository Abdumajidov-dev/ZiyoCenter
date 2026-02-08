using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using ZiyoMarket.Service.DTOs.Files;
using ZiyoMarket.Service.Enums;
using ZiyoMarket.Service.Helpers;
using ZiyoMarket.Service.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Service for handling file uploads with support for local and cloud storage
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly FileUploadSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileUploadService(
        IWebHostEnvironment webHostEnvironment,
        IOptions<FileUploadSettings> settings,
        IHttpContextAccessor httpContextAccessor)
    {
        _webHostEnvironment = webHostEnvironment;
        _settings = settings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FileUploadResultDto> UploadImageAsync(IFormFile file, ImageCategory category)
    {
        // Validate file
        var validationError = ValidateImage(file);
        if (validationError != null)
        {
            throw new Exception(validationError);
        }

        // Get category-specific path
        var categoryPath = GetCategoryPath(category);
        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, categoryPath);

        // Ensure directory exists
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Generate unique filename with .webp extension
        var uniqueFileName = $"{Guid.NewGuid()}.webp";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Convert to WebP and save
        using (var image = await Image.LoadAsync(file.OpenReadStream()))
        {
            // Optional: Resize if image is too large (e.g., max 1920x1920)
            if (image.Width > 1920 || image.Height > 1920)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(1920, 1920),
                    Mode = ResizeMode.Max
                }));
            }

            // Save as WebP with quality 80
            var encoder = new WebpEncoder
            {
                Quality = 80,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsync(filePath, encoder);
        }

        // Get file size after conversion
        var fileInfo = new FileInfo(filePath);
        var fileSize = fileInfo.Length;

        // Build relative path and URL
        var relativePath = $"{categoryPath}/{uniqueFileName}".Replace("\\", "/");
        var fileUrl = GetFileUrl(relativePath);

        return new FileUploadResultDto
        {
            FileName = uniqueFileName,
            FilePath = relativePath,
            FileUrl = fileUrl,
            FileSize = fileSize
        };
    }

    public async Task<List<FileUploadResultDto>> UploadImagesAsync(List<IFormFile> files, ImageCategory category)
    {
        var results = new List<FileUploadResultDto>();

        foreach (var file in files)
        {
            try
            {
                var result = await UploadImageAsync(file, category);
                results.Add(result);
            }
            catch (Exception ex)
            {
                // Log error but continue with other files
                // In production, use ILogger
                Console.WriteLine($"Failed to upload file {file.FileName}: {ex.Message}");
            }
        }

        return results;
    }

    public Task<bool> DeleteImageAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.FromResult(false);

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<int> DeleteImagesAsync(List<string> filePaths)
    {
        var deletedCount = 0;

        foreach (var filePath in filePaths)
        {
            var deleted = await DeleteImageAsync(filePath);
            if (deleted)
                deletedCount++;
        }

        return deletedCount;
    }

    public string? ValidateImage(IFormFile file)
    {
        // Check if file is null or empty
        if (file == null || file.Length == 0)
        {
            return "File is empty or not provided";
        }

        // Check file size
        if (file.Length > _settings.MaxFileSizeBytes)
        {
            var maxSizeMB = _settings.MaxFileSizeBytes / (1024 * 1024);
            return $"File size exceeds maximum allowed size of {maxSizeMB}MB";
        }

        // Check file extension
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_settings.AllowedImageExtensions.Contains(fileExtension))
        {
            return $"File extension {fileExtension} is not allowed. Allowed extensions: {string.Join(", ", _settings.AllowedImageExtensions)}";
        }

        // Check content type
        var allowedContentTypes = new[]
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/svg+xml"
        };

        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return $"File content type {file.ContentType} is not allowed";
        }

        return null; // Valid
    }

    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return string.Empty;

        // Normalize path separators
        filePath = filePath.Replace("\\", "/");

        // Get base URL from current request
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null)
        {
            var scheme = request.Scheme; // http or https
            var host = request.Host.Value; // localhost:8080 or production domain
            return $"{scheme}://{host}/{filePath}";
        }

        // Fallback for background jobs (where HttpContext is not available)
        // Use environment variable for production URL
        var baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:8080";
        return $"{baseUrl}/{filePath}";
    }

    private string GetCategoryPath(ImageCategory category)
    {
        return category switch
        {
            ImageCategory.Product => _settings.ProductImagesPath,
            ImageCategory.Category => _settings.CategoryImagesPath,
            ImageCategory.Banner => _settings.BannerImagesPath,
            ImageCategory.User => _settings.UserImagesPath,
            ImageCategory.Temp => _settings.TempImagesPath,
            _ => _settings.UploadPath
        };
    }
}
