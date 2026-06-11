using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Service.DTOs.Expenses;

public class CreateExpenseDto
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; } = ExpenseCategory.General;
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
}

public class UpdateExpenseDto
{
    public string? Title { get; set; }
    public decimal? Amount { get; set; }
    public ExpenseCategory? Category { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpenseDate { get; set; }
}

public class ExpenseResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

public class ExpenseCategorySummaryDto
{
    public ExpenseCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}

public class ExpenseSummaryDto
{
    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<ExpenseCategorySummaryDto> ByCategory { get; set; } = new();
}
