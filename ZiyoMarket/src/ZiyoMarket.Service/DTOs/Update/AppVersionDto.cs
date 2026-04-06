namespace ZiyoMarket.Service.DTOs.Update;

/// <summary>
/// App version information DTO
/// </summary>
public class AppVersionDto
{
    public int Id { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? DownloadUrl { get; set; }
    public long FileSize { get; set; }
    public string? Sha256Hash { get; set; }
    public bool IsCritical { get; set; }
    public bool IsActive { get; set; }
    public string Channel { get; set; } = "stable";
    public string? MinVersionRequired { get; set; }
    public string Platform { get; set; } = "windows";
    public string? FileName { get; set; }
    public string? CreatedAt { get; set; }
    public int DownloadCount { get; set; }
}
