using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Expenses;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IExpenseService
{
    Task<Result<PaginationResponse<ExpenseResultDto>>> GetPagedAsync(
        DateTime? from, DateTime? to, ExpenseCategory? category, int page, int pageSize);
    Task<Result<ExpenseResultDto>> GetByIdAsync(int id);
    Task<Result<ExpenseResultDto>> CreateAsync(CreateExpenseDto dto, int createdBy);
    Task<Result<ExpenseResultDto>> UpdateAsync(int id, UpdateExpenseDto dto, int updatedBy);
    Task<Result> DeleteAsync(int id);
    Task<Result<ExpenseSummaryDto>> GetSummaryAsync(DateTime from, DateTime to);
}
