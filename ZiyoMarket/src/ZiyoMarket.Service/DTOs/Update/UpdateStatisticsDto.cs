namespace ZiyoMarket.Service.DTOs.Update;

/// <summary>
/// Update download statistics
/// </summary>
public class UpdateStatisticsDto
{
    public int TotalDownloads { get; set; }
    public int TotalVersions { get; set; }
    public string? LatestVersion { get; set; }
    public int DownloadsToday { get; set; }
    public int DownloadsThisWeek { get; set; }
    public int DownloadsThisMonth { get; set; }
    public List<VersionDownloadStat> TopVersions { get; set; } = new();
}

public class VersionDownloadStat
{
    public string VersionNumber { get; set; } = string.Empty;
    public int DownloadCount { get; set; }
    public double Percentage { get; set; }
}
