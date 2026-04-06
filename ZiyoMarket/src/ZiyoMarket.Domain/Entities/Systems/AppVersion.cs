using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Systems;

/// <summary>
/// Application version information for auto-update system
/// </summary>
public class AppVersion : BaseEntity
{
    /// <summary>
    /// Version number in SemVer format (e.g., "1.2.0")
    /// </summary>
    public string VersionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Release date of this version
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Release notes in markdown format
    /// </summary>
    public string? ReleaseNotes { get; set; }

    /// <summary>
    /// Direct download URL or relative path
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// SHA256 hash for file integrity verification
    /// </summary>
    public string? Sha256Hash { get; set; }

    /// <summary>
    /// Is this a critical update? (Forces update)
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// Is this version active and available for download?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Release channel: stable, beta, dev
    /// </summary>
    public string Channel { get; set; } = "stable";

    /// <summary>
    /// Minimum version required to update to this version (optional)
    /// </summary>
    public string? MinVersionRequired { get; set; }

    /// <summary>
    /// Platform: windows, linux, macos
    /// </summary>
    public string Platform { get; set; } = "windows";

    /// <summary>
    /// File name of the installer/updater
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// User ID who created this version
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this version
    /// </summary>
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<UpdateDownload> Downloads { get; set; } = new List<UpdateDownload>();

    // Business logic methods
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void MarkAsCritical() => IsCritical = true;

    public void MarkAsUpdated(int userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Compare this version with another version string
    /// </summary>
    /// <param name="otherVersion">Version to compare with</param>
    /// <returns>True if this version is newer</returns>
    public bool IsNewerThan(string otherVersion)
    {
        return CompareVersions(VersionNumber, otherVersion) > 0;
    }

    /// <summary>
    /// Compare two version strings using SemVer
    /// </summary>
    private static int CompareVersions(string version1, string version2)
    {
        var v1Parts = ParseVersion(version1);
        var v2Parts = ParseVersion(version2);

        for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
        {
            int v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
            int v2Part = i < v2Parts.Length ? v2Parts[i] : 0;

            if (v1Part != v2Part)
                return v1Part.CompareTo(v2Part);
        }

        return 0;
    }

    private static int[] ParseVersion(string version)
    {
        // Remove pre-release suffix (e.g., "1.2.0-beta" -> "1.2.0")
        var cleanVersion = version.Split('-')[0];
        return cleanVersion.Split('.').Select(int.Parse).ToArray();
    }
}
