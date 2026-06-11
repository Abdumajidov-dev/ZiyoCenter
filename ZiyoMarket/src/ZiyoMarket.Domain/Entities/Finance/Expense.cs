using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Finance;

public class Expense : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseCategory Category { get; set; } = ExpenseCategory.General;
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
}
