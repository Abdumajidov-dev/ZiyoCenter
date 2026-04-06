namespace ZiyoMarket.Service.DTOs.Update;

/// <summary>
/// Request to check if new version is available
/// </summary>
public class CheckUpdateRequest
{
    /// <summary>
    /// Current version of the client app (e.g., "1.0.0")
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// Platform: windows, linux, macos
    /// </summary>
    public string Platform { get; set; } = "windows";

    /// <summary>
    /// Release channel: stable, beta, dev
    /// </summary>
    public string Channel { get; set; } = "stable";
}
