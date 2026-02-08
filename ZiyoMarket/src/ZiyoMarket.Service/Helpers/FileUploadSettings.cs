namespace ZiyoMarket.Service.Helpers;

/// <summary>
/// Configuration for file upload settings
/// </summary>
public class FileUploadSettings
{
    /// <summary>
    /// Maximum file size in bytes (default: 5MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB

    /// <summary>
    /// Allowed image extensions
    /// </summary>
    public string[] AllowedImageExtensions { get; set; } = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"
    };

    /// <summary>
    /// Upload directory path (relative to wwwroot)
    /// </summary>
    public string UploadPath { get; set; } = "images";

    /// <summary>
    /// Product images subdirectory
    /// </summary>
    public string ProductImagesPath { get; set; } = "images/products";

    /// <summary>
    /// Category images subdirectory
    /// </summary>
    public string CategoryImagesPath { get; set; } = "images/categories";

    /// <summary>
    /// Banner images subdirectory
    /// </summary>
    public string BannerImagesPath { get; set; } = "images/banners";

    /// <summary>
    /// User images (avatars) subdirectory
    /// </summary>
    public string UserImagesPath { get; set; } = "images/users";

    /// <summary>
    /// Temporary uploads subdirectory
    /// </summary>
    public string TempImagesPath { get; set; } = "images/temp";
}
