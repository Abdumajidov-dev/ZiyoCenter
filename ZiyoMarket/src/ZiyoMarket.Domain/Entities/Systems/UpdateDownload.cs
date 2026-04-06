using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Systems;

/// <summary>
/// Tracks download analytics for app updates
/// </summary>
public class UpdateDownload : BaseEntity
{
    /// <summary>
    /// Reference to the app version that was downloaded
    /// </summary>
    public int VersionId { get; set; }

    /// <summary>
    /// Date and time of download
    /// </summary>
    public DateTime DownloadDate { get; set; }

    /// <summary>
    /// Client's current version before update
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// IP address of the client (for analytics)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Platform: windows, linux, macos
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// Was the download completed successfully?
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// User agent string from the HTTP request
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Country code derived from IP (optional)
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Download duration in seconds (optional)
    /// </summary>
    public int? DownloadDuration { get; set; }

    // Navigation properties
    public virtual AppVersion? Version { get; set; }
}
