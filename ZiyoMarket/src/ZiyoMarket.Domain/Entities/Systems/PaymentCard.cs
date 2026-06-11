using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Systems;

public class PaymentCard : BaseEntity
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsActive { get; set; } = false;

    public string MaskedCardNumber => CardNumber.Length >= 16
        ? $"{CardNumber[..4]} **** **** {CardNumber[^4..]}"
        : CardNumber;
}
