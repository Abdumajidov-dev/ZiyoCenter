using ZiyoMarket.Service.DTOs.Support;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface ISupportService
{
    Task<Result<SupportChatDetailDto>> GetChatByIdAsync(int chatId);
    Task<Result<PaginationResponse<SupportChatListDto>>> GetChatsAsync(SupportChatFilterRequest request);
    Task<Result<SupportChatDetailDto>> CreateChatAsync(CreateChatDto request, int customerId);
    Task<Result> CloseChatAsync(int chatId, string closeReason, int closedBy);
    Task<Result> ReopenChatAsync(int chatId, int reopenedBy);
    Task<Result> AssignChatAsync(int chatId, int adminId, int assignedBy);
    Task<Result> UnassignChatAsync(int chatId, int unassignedBy);

    // Message operations
    Task<Result<List<SupportMessageDto>>> GetChatMessagesAsync(int chatId, int pageNumber = 1, int pageSize = 50);
    Task<Result<SupportMessageDto>> SendMessageAsync(SendMessageDto request, int senderId, string senderType);
    Task<Result> DeleteMessageAsync(int messageId, int deletedBy);
    Task<Result> EditMessageAsync(int messageId, string newMessage, int editedBy);
    Task<Result> MarkMessageAsReadAsync(int messageId, int userId);

    // Customer support
    Task<Result<List<SupportChatListDto>>> GetCustomerChatsAsync(int customerId);
    Task<Result<CustomerSupportStatsDto>> GetCustomerSupportStatsAsync(int customerId);
    Task<Result<SupportChatDetailDto>> GetLatestCustomerChatAsync(int customerId);

    // Admin support
    Task<Result<List<SupportChatListDto>>> GetAdminChatsAsync(int adminId);
    Task<Result<AdminSupportStatsDto>> GetAdminSupportStatsAsync(int adminId);
    Task<Result<List<SupportChatListDto>>> GetUnassignedChatsAsync();
    Task<Result<List<SupportChatListDto>>> GetOverdueChatsAsync();

    // Chat feedback
    Task<Result> SubmitChatFeedbackAsync(int chatId, SubmitFeedbackDto request);
    Task<Result<List<ChatFeedbackDto>>> GetChatFeedbackAsync(DateTime startDate, DateTime endDate);
    Task<Result<FeedbackStatsDto>> GetFeedbackStatisticsAsync(DateTime startDate, DateTime endDate);

    // Chat statistics
    Task<Result<SupportStatsDto>> GetSupportStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<Result<List<ChatResponseTimeDto>>> GetResponseTimeStatsAsync(DateTime startDate, DateTime endDate);
    Task<Result<List<ChatResolutionTimeDto>>> GetResolutionTimeStatsAsync(DateTime startDate, DateTime endDate);

    // Tags and categories
    Task<Result<List<string>>> GetAllChatTagsAsync();
    Task<Result<List<string>>> GetAllChatCategoriesAsync();
    Task<Result<List<SupportChatListDto>>> GetChatsByTagAsync(string tag);
    Task<Result<List<SupportChatListDto>>> GetChatsByCategoryAsync(string category);

    // Bulk operations
    Task<Result> DeleteAllChatsAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<SupportChatDetailDto>>> SeedMockChatsAsync(int createdBy, int count = 10);
    Task<Result<List<SupportMessageDto>>> SeedMockMessagesAsync(int chatId, int createdBy, int count = 10);
}