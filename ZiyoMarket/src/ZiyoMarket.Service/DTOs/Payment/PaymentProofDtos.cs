using Microsoft.AspNetCore.Http;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Service.DTOs.Payment;

/// <summary>
/// To'lov isbotini yuklash uchun DTO
/// </summary>
public class SubmitPaymentProofDto
{
    /// <summary>
    /// Buyurtma ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// To'lov summasi
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction reference (ixtiyoriy)
    /// </summary>
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Jo'natuvchi karta raqami (oxirgi 4 ta raqam: **** 1234)
    /// </summary>
    public string? SenderCardNumber { get; set; }

    /// <summary>
    /// To'lov sanasi (format: "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    public string? PaymentDate { get; set; }

    /// <summary>
    /// To'lov isboti screenshot'i
    /// </summary>
    public IFormFile? ProofImage { get; set; }

    /// <summary>
    /// Mijoz izohi
    /// </summary>
    public string? CustomerNotes { get; set; }
}

/// <summary>
/// Admin to'lovni tasdiqlash uchun DTO
/// </summary>
public class VerifyPaymentProofDto
{
    /// <summary>
    /// Admin izohi
    /// </summary>
    public string? AdminNotes { get; set; }
}

/// <summary>
/// Admin to'lovni rad etish uchun DTO
/// </summary>
public class RejectPaymentProofDto
{
    /// <summary>
    /// Rad etish sababi (majburiy)
    /// </summary>
    public string AdminNotes { get; set; } = string.Empty;
}

/// <summary>
/// To'lov isboti natija DTO
/// </summary>
public class PaymentProofResultDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionReference { get; set; }
    public string? SenderCardNumber { get; set; }
    public string? ProofImageUrl { get; set; }
    public string? PaymentDate { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string? CustomerNotes { get; set; }
    public string? AdminNotes { get; set; }
    public string? ReviewedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public string? ReviewedByName { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? UpdatedAt { get; set; }
}

/// <summary>
/// Admin karta ma'lumotlari DTO
/// </summary>
public class BankTransferInfoDto
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string Instructions { get; set; } = "Ushbu karta raqamiga pul o'tkazing va screenshot yuklang";
}

/// <summary>
/// To'lov isbotlari statistikasi (Admin uchun)
/// </summary>
public class PaymentProofStatsDto
{
    public int PendingCount { get; set; }
    public decimal PendingAmount { get; set; }
    public int VerifiedCount { get; set; }
    public decimal VerifiedAmount { get; set; }
    public int RejectedCount { get; set; }
    public decimal RejectedAmount { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// To'lov isbotini yuklash natijasi
/// </summary>
public class SubmitPaymentProofResultDto
{
    public int PaymentProofId { get; set; }
    public int OrderId { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = string.Empty;
    public string EstimatedReviewTime { get; set; } = "1-24 soat";
}
