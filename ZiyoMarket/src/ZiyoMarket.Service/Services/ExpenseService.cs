using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Finance;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Expenses;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExpenseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginationResponse<ExpenseResultDto>>> GetPagedAsync(
        DateTime? from, DateTime? to, ExpenseCategory? category, int page, int pageSize)
    {
        try
        {
            var (items, total) = await _unitOfWork.Expenses.GetPagedAsync(
                page, pageSize,
                filter: e =>
                    (from == null || e.ExpenseDate >= from) &&
                    (to == null || e.ExpenseDate <= to) &&
                    (category == null || e.Category == category),
                orderBy: q => q.OrderByDescending(e => e.ExpenseDate));

            var dtos = items.Select(ToDto).ToList();
            return Result<PaginationResponse<ExpenseResultDto>>.Success(
                new PaginationResponse<ExpenseResultDto>(dtos, total, page, pageSize));
        }
        catch (Exception ex)
        {
            return Result<PaginationResponse<ExpenseResultDto>>.InternalError(ex.Message);
        }
    }

    public async Task<Result<ExpenseResultDto>> GetByIdAsync(int id)
    {
        try
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense is null)
                return Result<ExpenseResultDto>.NotFound($"Xarajat topilmadi (id={id})");

            return Result<ExpenseResultDto>.Success(ToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseResultDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result<ExpenseResultDto>> CreateAsync(CreateExpenseDto dto, int createdBy)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return Result<ExpenseResultDto>.Failure("Nomi kiritilishi shart");
            if (dto.Amount <= 0)
                return Result<ExpenseResultDto>.Failure("Summa 0 dan katta bo'lishi kerak");

            var expense = new Expense
            {
                Title = dto.Title.Trim(),
                Amount = dto.Amount,
                Category = dto.Category,
                Description = dto.Description?.Trim(),
                ExpenseDate = dto.ExpenseDate.Date,
                CreatedBy = createdBy
            };

            await _unitOfWork.Expenses.InsertAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return Result<ExpenseResultDto>.Success(ToDto(expense), "Xarajat qo'shildi", 201);
        }
        catch (Exception ex)
        {
            return Result<ExpenseResultDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result<ExpenseResultDto>> UpdateAsync(int id, UpdateExpenseDto dto, int updatedBy)
    {
        try
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense is null)
                return Result<ExpenseResultDto>.NotFound($"Xarajat topilmadi (id={id})");

            if (dto.Title != null) expense.Title = dto.Title.Trim();
            if (dto.Amount.HasValue)
            {
                if (dto.Amount.Value <= 0)
                    return Result<ExpenseResultDto>.Failure("Summa 0 dan katta bo'lishi kerak");
                expense.Amount = dto.Amount.Value;
            }
            if (dto.Category.HasValue) expense.Category = dto.Category.Value;
            if (dto.Description != null) expense.Description = dto.Description.Trim();
            if (dto.ExpenseDate.HasValue) expense.ExpenseDate = dto.ExpenseDate.Value.Date;
            expense.UpdatedBy = updatedBy;

            await _unitOfWork.SaveChangesAsync();

            return Result<ExpenseResultDto>.Success(ToDto(expense));
        }
        catch (Exception ex)
        {
            return Result<ExpenseResultDto>.InternalError(ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense is null)
                return Result.NotFound($"Xarajat topilmadi (id={id})");

            _unitOfWork.Expenses.SoftDelete(expense);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Xarajat o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError(ex.Message);
        }
    }

    public async Task<Result<ExpenseSummaryDto>> GetSummaryAsync(DateTime from, DateTime to)
    {
        try
        {
            var expenses = await _unitOfWork.Expenses.FindAsync(
                e => e.ExpenseDate >= from && e.ExpenseDate <= to);

            var list = expenses.ToList();
            var byCategory = list
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseCategorySummaryDto
                {
                    Category = g.Key,
                    CategoryName = GetCategoryName(g.Key),
                    TotalAmount = g.Sum(e => e.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            var summary = new ExpenseSummaryDto
            {
                TotalAmount = list.Sum(e => e.Amount),
                TotalCount = list.Count,
                From = from,
                To = to,
                ByCategory = byCategory
            };

            return Result<ExpenseSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return Result<ExpenseSummaryDto>.InternalError(ex.Message);
        }
    }

    private static ExpenseResultDto ToDto(Expense e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Amount = e.Amount,
        Category = e.Category,
        CategoryName = GetCategoryName(e.Category),
        Description = e.Description,
        ExpenseDate = e.ExpenseDate,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static string GetCategoryName(ExpenseCategory category) => category switch
    {
        ExpenseCategory.Salary => "Maosh",
        ExpenseCategory.Rent => "Ijara",
        ExpenseCategory.Utilities => "Kommunal",
        ExpenseCategory.Marketing => "Marketing",
        ExpenseCategory.Logistics => "Logistika",
        ExpenseCategory.Equipment => "Jihozlar",
        ExpenseCategory.Other => "Boshqa",
        _ => "Umumiy"
    };
}
