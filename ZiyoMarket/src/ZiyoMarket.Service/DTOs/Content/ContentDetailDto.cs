// ZiyoMarket.Service/DTOs/Content/ContentDtos.cs

using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Service.DTOs.Content;

// ============ MAIN CONTENT DTOS ============

/// <summary>
/// Content detail DTO - to'liq ma'lumot
/// </summary>
public class ContentDetailDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // Banner, Video, Article, Announcement
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; } // HTML content for articles
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ExternalUrl { get; set; }

    // Display settings
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int SortOrder { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }

    // Targeting
    public string TargetAudience { get; set; } = "All"; // All, Customers, Sellers
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Additional metadata
    public string? Author { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public int? DurationSeconds { get; set; } // For videos
    public string? ThumbnailUrl { get; set; }

    // Status
    public bool IsActive => IsPublished &&
                           (!StartDate.HasValue || StartDate.Value <= DateTime.UtcNow) &&
                           (!EndDate.HasValue || EndDate.Value >= DateTime.UtcNow);

    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    public bool IsScheduled => StartDate.HasValue && StartDate.Value > DateTime.UtcNow;

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// Content list DTO - ro'yxat uchun
/// </summary>
public class ContentListDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int SortOrder { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public string TargetAudience { get; set; } = "All";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Save content DTO - create/update uchun
/// </summary>
public class SaveContentDto
{
    public string Type { get; set; } = string.Empty; // Banner, Video, Article, Announcement
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; } // HTML for articles
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? ExternalUrl { get; set; }

    // Display settings
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }

    // Targeting
    public string TargetAudience { get; set; } = "All";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Additional metadata
    public string? Author { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public int? DurationSeconds { get; set; }
    public string? ThumbnailUrl { get; set; }
}

/// <summary>
/// Content filter request - filter va pagination
/// </summary>
public class ContentFilterRequest : PaginationRequest
{
    public string? Type { get; set; } // Banner, Video, Article, Announcement
    public bool? IsPublished { get; set; }
    public string? TargetAudience { get; set; }
    public string? SearchTerm { get; set; }
    public string? Tag { get; set; }
    public string? Category { get; set; }
    public string? Author { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsExpired { get; set; }
    public bool? IsScheduled { get; set; }
    public string? SortBy { get; set; } // title, publishedAt, viewCount, sortOrder
    public bool SortDescending { get; set; }
    public bool IsActive { get; internal set; }
}

/// <summary>
/// Update content order DTO
/// </summary>
public class UpdateContentOrderDto
{
    public int ContentId { get; set; }
    public int NewSortOrder { get; set; }
    public int DisplayOrder { get; internal set; }
}

// ============ TYPE SPECIFIC DTOS ============

/// <summary>
/// Banner DTO
/// </summary>
public class BannerDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ExternalUrl { get; set; }
    public int SortOrder { get; set; }
    public string TargetAudience { get; set; } = "All";
    public int ClickCount { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Video DTO
/// </summary>
public class VideoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Author { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Article DTO
/// </summary>
public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Content { get; set; } = string.Empty; // HTML content
    public string? ImageUrl { get; set; }
    public string? Author { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public int EstimatedReadTimeMinutes => CalculateReadTime(Content);

    private int CalculateReadTime(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return 0;
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(1, wordCount / 200); // Average 200 words per minute
    }
}

/// <summary>
/// Announcement DTO
/// </summary>
public class AnnouncementDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TargetAudience { get; set; } = "All";
    public int ViewCount { get; set; }
    public bool IsActive { get; set; }
}

// ============ STATISTICS & ANALYTICS DTOS ============

/// <summary>
/// Content statistics DTO
/// </summary>
public class ContentStatsDto
{
    public int TotalContent { get; set; }
    public int PublishedContent { get; set; }
    public int DraftContent { get; set; }
    public int ScheduledContent { get; set; }
    public int ExpiredContent { get; set; }

    // By type
    public int TotalBanners { get; set; }
    public int TotalVideos { get; set; }
    public int TotalArticles { get; set; }
    public int TotalAnnouncements { get; set; }

    // Engagement
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double AverageViewsPerContent { get; set; }
    public double AverageClicksPerContent { get; set; }

    // Top performers
    public string? MostViewedContentTitle { get; set; }
    public int MostViewedContentViews { get; set; }
    public string? MostClickedContentTitle { get; set; }
    public int MostClickedContentClicks { get; set; }
    public int ActiveContent { get; internal set; }
    public object ContentByType { get; internal set; }
}

/// <summary>
/// Top content DTO
/// </summary>
public class TopContentDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public double EngagementRate { get; set; } // (Clicks / Views) * 100
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; internal set; }
    public int ContentId { get; internal set; }
}

/// <summary>
/// Content performance DTO
/// </summary>
public class ContentPerformanceDto
{
    public int ContentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    // Engagement metrics
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double ClickThroughRate { get; set; } // (Clicks / Views) * 100

    // Time-based metrics
    public int ViewsToday { get; set; }
    public int ViewsThisWeek { get; set; }
    public int ViewsThisMonth { get; set; }
    public int ClicksToday { get; set; }
    public int ClicksThisWeek { get; set; }
    public int ClicksThisMonth { get; set; }

    // Average metrics
    public double AverageDailyViews { get; set; }
    public double AverageDailyClicks { get; set; }

    // Ranking
    public int ViewRank { get; set; } // Rank among all content
    public int ClickRank { get; set; }

    // Publishing info
    public DateTime? PublishedAt { get; set; }
    public int DaysSincePublished { get; set; }
    public int ViewCount { get; internal set; }
    public int ClickCount { get; internal set; }
}

/// <summary>
/// Content view statistics DTO
/// </summary>
public class ContentViewStatsDto
{
    public DateTime Date { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public int UniqueContent { get; set; }
    public double AverageViewsPerContent { get; set; }
    public double AverageClicksPerContent { get; set; }

    // By type
    public int BannerViews { get; set; }
    public int VideoViews { get; set; }
    public int ArticleViews { get; set; }
    public int AnnouncementViews { get; set; }
    public int ContentId { get; internal set; }
    public string Title { get; internal set; }
    public string Type { get; internal set; }
    public int ClickCount { get; internal set; }
    public int ViewCount { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
}

// ============ ADDITIONAL DTOS ============

/// <summary>
/// Content tag DTO
/// </summary>
public class ContentTagDto
{
    public string Tag { get; set; } = string.Empty;
    public int ContentCount { get; set; }
}

/// <summary>
/// Content category DTO
/// </summary>
public class ContentCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public int ContentCount { get; set; }
}

/// <summary>
/// Content author DTO
/// </summary>
public class ContentAuthorDto
{
    public string Author { get; set; } = string.Empty;
    public int ContentCount { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
}

/// <summary>
/// Bulk content operation result DTO
/// </summary>
public class BulkContentOperationResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<int> ProcessedIds { get; set; } = new();
}

/// <summary>
/// Content preview DTO (for drafts)
/// </summary>
public class ContentPreviewDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = "All";
}

/// <summary>
/// Content publishing schedule DTO
/// </summary>
public class ContentPublishingScheduleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime? ScheduledPublishDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public string Status { get; set; } = string.Empty; // Scheduled, Published, Expired
    public int DaysUntilPublish { get; set; }
    public int DaysUntilExpiry { get; set; }
}

/// <summary>
/// Content engagement summary DTO
/// </summary>
public class ContentEngagementSummaryDto
{
    public DateTime Date { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double ClickThroughRate { get; set; }
    public int ActiveContent { get; set; }
    public string MostPopularContentType { get; set; } = string.Empty;
    public int MostPopularContentTypeViews { get; set; }
}

/// <summary>
/// Content search result DTO
/// </summary>
public class ContentSearchResultDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string MatchType { get; set; } = string.Empty; // Title, Description, Content, Tag
    public double RelevanceScore { get; set; }
}

/// <summary>
/// Content audit log DTO
/// </summary>
public class ContentAuditLogDto
{
    public int ContentId { get; set; }
    public string ContentTitle { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Created, Updated, Published, Unpublished, Deleted
    public string? Changes { get; set; } // JSON string of changes
    public DateTime Timestamp { get; set; }
    public int? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }
}

/// <summary>
/// Content export DTO
/// </summary>
public class ContentExportDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public string? Author { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Content validation result DTO
/// </summary>
public class ContentValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, string> ValidationDetails { get; set; } = new();
}

// ============ PAGINATION DTO ============

/// <summary>
/// Pagination request base
/// </summary>
public class PaginationRequest
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public int Skip => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Pagination response
/// </summary>
public class PaginationResponse<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginationResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}