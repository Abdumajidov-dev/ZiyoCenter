using System.ComponentModel.DataAnnotations.Schema;
using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// To'lov isboti - Manual Payment Verification uchun
/// Mijoz to'g'ridan-to'g'ri admin kartasiga o'tkazib, screenshot yoki ma'lumot yuboradi
/// </summary>
public class PaymentProof : BaseAuditableEntity
{
    /// <summary>
    /// Qaysi buyurtmaga tegishli
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// To'lov qilgan mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// To'lov usuli
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// To'lov summasi (mijoz kiritadi)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaksiya raqami / Reference Number (ixtiyoriy)
    /// Masalan: Bank o'tkazmasidagi reference number
    /// </summary>
    public string? TransactionReference { get; set; }

    /// <summary>
    /// Jo'natuvchi karta raqami (oxirgi 4 ta raqam masalan: **** 1234)
    /// </summary>
    public string? SenderCardNumber { get; set; }

    /// <summary>
    /// To'lov isboti screenshot'i (wwwroot/images/payment_proofs/)
    /// </summary>
    public string? ProofImageUrl { get; set; }

    /// <summary>
    /// To'lov sanasi (mijoz to'lov qilgan vaqt - mijoz kiritadi)
    /// </summary>
    public string? PaymentDate { get; set; }

    /// <summary>
    /// To'lov holati
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.UnderReview;

    /// <summary>
    /// Mijoz izohi (ixtiyoriy)
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Admin izohi (tasdiqlash/rad etish sababi)
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// Admin tomonidan ko'rilgan vaqt
    /// </summary>
    public string? ReviewedAt { get; set; }

    /// <summary>
    /// Qaysi admin ko'rib chiqdi
    /// </summary>
    public int? ReviewedBy { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Buyurtma
    /// </summary>
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;

    // ==================== METHODS ====================

    /// <summary>
    /// To'lovni tasdiqlash (Admin)
    /// </summary>
    public void Approve(int adminId, string? notes = null)
    {
        if (Status != PaymentStatus.UnderReview)
            throw new InvalidOperationException("Faqat ko'rib chiqilayotgan to'lovni tasdiqlash mumkin");

        Status = PaymentStatus.Verified;
        AdminNotes = notes;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        UpdatedBy = adminId;
        MarkAsUpdated();
    }

    /// <summary>
    /// To'lovni rad etish (Admin)
    /// </summary>
    public void Reject(int adminId, string reason)
    {
        if (Status != PaymentStatus.UnderReview)
            throw new InvalidOperationException("Faqat ko'rib chiqilayotgan to'lovni rad etish mumkin");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rad etish sababi kiritilishi shart");

        Status = PaymentStatus.Rejected;
        AdminNotes = reason;
        ReviewedBy = adminId;
        ReviewedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        UpdatedBy = adminId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (OrderId <= 0)
            errors.Add("Buyurtma ID noto'g'ri");

        if (CustomerId <= 0)
            errors.Add("Mijoz ID noto'g'ri");

        if (Amount <= 0)
            errors.Add("To'lov summasi musbat bo'lishi kerak");

        if (string.IsNullOrWhiteSpace(ProofImageUrl) && string.IsNullOrWhiteSpace(TransactionReference))
            errors.Add("To'lov isboti (screenshot yoki transaction reference) kiritilishi shart");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Status matni
    /// </summary>
    public string GetStatusText()
    {
        return Status switch
        {
            PaymentStatus.Pending => "Kutilmoqda",
            PaymentStatus.AwaitingProof => "Isboti kutilmoqda",
            PaymentStatus.UnderReview => "Ko'rib chiqilmoqda",
            PaymentStatus.Verified => "Tasdiqlandi",
            PaymentStatus.Rejected => "Rad etildi",
            PaymentStatus.Refunded => "Qaytarildi",
            _ => Status.ToString()
        };
    }
}
