using Microsoft.AspNetCore.Http;

namespace ZiyoMarket.Service.DTOs.Update;

/// <summary>
/// Request to create new app version (Admin only)
/// </summary>
public class CreateAppVersionRequest
{
    /// <summary>
    /// Version number (e.g., "1.2.0")
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Release notes in markdown format
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Is this a critical update?
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// Release channel: stable, beta, dev
    /// </summary>
    public string Channel { get; set; } = "stable";

    /// <summary>
    /// Minimum version required
    /// </summary>
    public string? MinVersionRequired { get; set; }

    /// <summary>
    /// Platform: windows, linux, macos
    /// </summary>
    public string Platform { get; set; } = "windows";

    /// <summary>
    /// Installer file
    /// </summary>
    public IFormFile? File { get; set; }
}
