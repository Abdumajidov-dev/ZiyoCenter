using ZiyoMarket.Service.DTOs.Update;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Service for managing application auto-update system
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Check if new version is available for the client
    /// </summary>
    Task<Result<UpdateInfoDto>> CheckForUpdatesAsync(CheckUpdateRequest request);

    /// <summary>
    /// Get latest version for specific channel
    /// </summary>
    Task<Result<AppVersionDto>> GetLatestVersionAsync(string channel = "stable", string platform = "windows");

    /// <summary>
    /// Get all releases for a channel
    /// </summary>
    Task<Result<List<AppVersionDto>>> GetAllReleasesAsync(string? channel = null, string? platform = null);

    /// <summary>
    /// Get version by ID
    /// </summary>
    Task<Result<AppVersionDto>> GetVersionByIdAsync(int id);

    /// <summary>
    /// Create new version (Admin only)
    /// </summary>
    Task<Result<AppVersionDto>> CreateVersionAsync(CreateAppVersionRequest request, int createdBy);

    /// <summary>
    /// Update version information (Admin only)
    /// </summary>
    Task<Result<AppVersionDto>> UpdateVersionAsync(int id, CreateAppVersionRequest request, int updatedBy);

    /// <summary>
    /// Delete version (soft delete, Admin only)
    /// </summary>
    Task<Result> DeleteVersionAsync(int id, int deletedBy);

    /// <summary>
    /// Record download analytics
    /// </summary>
    Task<Result> RecordDownloadAsync(int versionId, string? clientVersion, string? ipAddress, string? userAgent);

    /// <summary>
    /// Get download statistics (Admin only)
    /// </summary>
    Task<Result<UpdateStatisticsDto>> GetStatisticsAsync();

    /// <summary>
    /// Get file path for download
    /// </summary>
    Task<Result<string>> GetDownloadFilePathAsync(int versionId);
}
