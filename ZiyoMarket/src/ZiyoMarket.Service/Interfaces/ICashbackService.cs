using ZiyoMarket.Service.DTOs.Cashback;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface ICashbackService
{
    Task<Result<CashbackSummaryDto>> GetCashbackSummaryAsync(int customerId);
    Task<Result<List<CashbackTransactionDto>>> GetCashbackHistoryAsync(int customerId, int pageNumber = 1, int pageSize = 20);
    Task<Result> EarnCashbackAsync(int customerId, int orderId, decimal amount);
    Task<Result> UseCashbackAsync(int customerId, int orderId, decimal amount);
    Task<Result> ExpireCashbackAsync();
    Task<Result<decimal>> GetAvailableCashbackAsync(int customerId);
    Task<Result<decimal>> GetExpiringCashbackAsync(int customerId, int daysThreshold = 7);

    // Bulk operations
    Task<Result> DeleteAllCashbackTransactionsAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<CashbackTransactionDto>>> SeedMockCashbackAsync(int customerId, int count = 10);
}