using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IContentService
{
    // CRUD operations
    Task<Result<ContentDetailDto>> GetContentByIdAsync(int contentId);
    Task<Result<DTOs.Content.PaginationResponse<ContentListDto>>> GetAllContentAsync(ContentFilterRequest request);
    Task<Result<ContentDetailDto>> CreateContentAsync(SaveContentDto request, int createdBy);
    Task<Result<ContentDetailDto>> UpdateContentAsync(int id, SaveContentDto request, int updatedBy);
    Task<Result> DeleteContentAsync(int contentId, int deletedBy);

    // Publishing operations
    Task<Result> PublishContentAsync(int contentId, int publishedBy);
    Task<Result> UnpublishContentAsync(int contentId, int unpublishedBy);
    Task<Result<List<ContentListDto>>> GetPublishedContentAsync(string? type = null);
    Task<Result<List<ContentListDto>>> GetScheduledContentAsync();
    Task<Result> UpdateContentOrderAsync(List<UpdateContentOrderDto> updates, int updatedBy);

// Content type specific operations
    Task<Result<List<ContentListDto>>> GetContentByTypeAsync(string type);
    Task<Result<List<BannerDto>>> GetActiveBannersAsync();
    Task<Result<List<VideoDto>>> GetLatestVideosAsync(int count = 10);
    Task<Result<List<ArticleDto>>> GetLatestArticlesAsync(int count = 10);
    Task<Result<List<AnnouncementDto>>> GetActiveAnnouncementsAsync();
    
    // Analytics and statistics
    Task<Result<ContentStatsDto>> GetContentStatisticsAsync();
    Task<Result<List<TopContentDto>>> GetTopContentAsync(DateTime? startDate = null, DateTime? endDate = null, int count = 10);
    Task<Result<ContentPerformanceDto>> GetContentPerformanceAsync(int contentId);

    // View tracking
    Task<Result> IncrementViewCountAsync(int contentId);
    Task<Result> IncrementClickCountAsync(int contentId);
    Task<Result<List<ContentViewStatsDto>>> GetViewStatisticsAsync(DateTime startDate, DateTime endDate);

    // Content management
    Task<Result<List<ContentListDto>>> GetExpiredContentAsync();
    Task<Result<List<ContentListDto>>> GetDraftContentAsync();
    Task<Result> ArchiveOldContentAsync(DateTime olderThan, int archivedBy);

    // Search and filter
    Task<Result<List<ContentListDto>>> SearchContentAsync(string searchTerm);
    Task<Result<List<ContentListDto>>> GetContentByTagAsync(string tag);
    Task<Result<List<string>>> GetAllContentTagsAsync();
    Task<Result<List<ContentListDto>>> GetContentByAuthorAsync(string author);

    // Bulk operations
    Task<Result> DeleteAllContentAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<ContentDetailDto>>> SeedMockContentAsync(int createdBy, int count = 10);
}