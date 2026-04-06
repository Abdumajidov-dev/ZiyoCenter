namespace ZiyoMarket.Service.DTOs.Update;

/// <summary>
/// Update information response
/// </summary>
public class UpdateInfoDto
{
    /// <summary>
    /// Is update available?
    /// </summary>
    public bool UpdateAvailable { get; set; }

    /// <summary>
    /// Latest version available on server
    /// </summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>
    /// Client's current version
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Release date of the latest version
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Direct download URL
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// Release notes in markdown format
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Is this a critical update? (Forces update)
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// SHA256 hash for integrity verification
    /// </summary>
    public string? Sha256Hash { get; set; }

    /// <summary>
    /// Minimum version required to update to this version
    /// </summary>
    public string? MinVersionRequired { get; set; }

    /// <summary>
    /// File name of the installer
    /// </summary>
    public string? FileName { get; set; }
}
