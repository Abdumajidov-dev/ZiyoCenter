namespace ZiyoMarket.Service.DTOs.Files;

/// <summary>
/// Result of file upload operation
/// </summary>
public class FileUploadResultDto
{
    /// <summary>
    /// File name (generated unique name)
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File path relative to wwwroot (e.g., "images/products/abc123.jpg")
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Full URL to access the file (e.g., "http://localhost:8080/images/products/abc123.jpg")
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Upload timestamp
    /// </summary>
    public string UploadedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}
