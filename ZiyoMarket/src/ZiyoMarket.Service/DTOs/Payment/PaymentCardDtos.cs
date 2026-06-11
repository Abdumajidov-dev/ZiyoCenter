using System.ComponentModel.DataAnnotations;

namespace ZiyoMarket.Service.DTOs.Payment;

public class CreatePaymentCardDto
{
    [Required]
    [StringLength(19, MinimumLength = 16)]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CardHolder { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string BankName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Note { get; set; }
}

public class PaymentCardResultDto
{
    public int Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string MaskedCardNumber { get; set; } = string.Empty;
    public string CardHolder { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
