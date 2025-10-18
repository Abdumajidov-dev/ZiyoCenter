using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// Cashback tranzaksiya entity'si - FIFO tizimi bilan
/// </summary>
public class CashbackTransaction : BaseEntity
{
    // ==================== PROPERTIES ====================

    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Buyurtma ID (qaysi buyurtmadan yig'ildi)
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Tranzaksiya turi: Earned, Used, Expired
    /// </summary>
    public CashbackTransactionType Type { get; set; }

    /// <summary>
    /// Cashback summasi
    /// - Earned: musbat (masalan: +5000)
    /// - Used: manfiy (masalan: -3000)
    /// - Expired: manfiy (masalan: -2000)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Qolgan summa (FIFO uchun)
    /// - Earned: Amount bilan bir xil boshlanadi
    /// - Ishlatilgan sari kamayadi
    /// - 0 bo'lsa to'liq ishlatilgan yoki muddati tugagan
    /// </summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// Yig'ilgan sana (string format: "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    public string EarnedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Amal qilish muddati (30 kun)
    /// String format: "yyyy-MM-dd HH:mm:ss"
    /// </summary>
    public string ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// Ishlatilgan sana (string format: "yyyy-MM-dd HH:mm:ss")
    /// </summary>
    public string? UsedAt { get; set; }

    /// <summary>
    /// Izoh/Tavsif
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tranzaksiya raqami (unique)
    /// Format: CB-20250118-123456
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Mijoz
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Buyurtma (cashback yig'ilgan)
    /// </summary>
    public virtual Order? Order { get; set; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Cashback faolmi (muddati tugamaganmi va ishlatilmaganmi)
    /// </summary>
    public bool IsActive => Type == CashbackTransactionType.Earned &&
                           RemainingAmount > 0 &&
                           DateTime.Parse(ExpiresAt) > DateTime.UtcNow &&
                           !IsDeleted;

    /// <summary>
    /// Cashback ishlatilganmi (to'liq)
    /// </summary>
    public bool IsUsed => Type == CashbackTransactionType.Earned &&
                         RemainingAmount == 0 &&
                         !string.IsNullOrEmpty(UsedAt);

    /// <summary>
    /// Cashback muddati tugaganmi
    /// </summary>
    public bool IsExpired => Type == CashbackTransactionType.Earned &&
                            DateTime.Parse(ExpiresAt) <= DateTime.UtcNow &&
                            RemainingAmount > 0;

    /// <summary>
    /// Ishlatish uchun mavjudmi (FIFO uchun)
    /// </summary>
    public bool IsAvailableForUse => IsActive && !IsUsed && !IsExpired;

    // ==================== STATIC FACTORY METHODS ====================

    /// <summary>
    /// Tranzaksiya raqamini generate qilish
    /// Format: CB-20250118-123456
    /// </summary>
    public static string GenerateTransactionNumber()
    {
        var today = DateTime.UtcNow;
        var random = new Random().Next(100000, 999999);
        return $"CB-{today:yyyyMMdd}-{random}";
    }

    /// <summary>
    /// Earned cashback yaratish (buyurtmadan keyin)
    /// </summary>
    /// <param name="customerId">Mijoz ID</param>
    /// <param name="orderId">Buyurtma ID</param>
    /// <param name="amount">Cashback summasi (2% dan)</param>
    /// <param name="description">Izoh</param>
    /// <returns>Yangi CashbackTransaction entity</returns>
    public static CashbackTransaction CreateEarned(
        int customerId,
        int orderId,
        decimal amount,
        string? description = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Cashback summasi musbat bo'lishi kerak", nameof(amount));

        if (customerId <= 0)
            throw new ArgumentException("Mijoz ID noto'g'ri", nameof(customerId));

        if (orderId <= 0)
            throw new ArgumentException("Buyurtma ID noto'g'ri", nameof(orderId));

        var now = DateTime.UtcNow;
        var nowString = now.ToString("yyyy-MM-dd HH:mm:ss");
        var expiresAtString = now.AddDays(30).ToString("yyyy-MM-dd HH:mm:ss");

        return new CashbackTransaction
        {
            CustomerId = customerId,
            OrderId = orderId,
            Type = CashbackTransactionType.Earned,
            Amount = amount,
            RemainingAmount = amount, // To'liq summa mavjud
            EarnedAt = nowString,
            ExpiresAt = expiresAtString,
            Description = description ?? $"Earned from Order #{orderId}",
            TransactionNumber = GenerateTransactionNumber()
        };
    }

    /// <summary>
    /// Used cashback yaratish (buyurtmada ishlatilganda)
    /// </summary>
    /// <param name="customerId">Mijoz ID</param>
    /// <param name="orderId">Qaysi buyurtmada ishlatildi</param>
    /// <param name="amount">Ishlatilgan summa (musbat)</param>
    /// <param name="description">Izoh</param>
    /// <returns>Yangi CashbackTransaction entity (Used)</returns>
    public static CashbackTransaction CreateUsed(
        int customerId,
        int orderId,
        decimal amount,
        string? description = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Ishlatilgan summa musbat bo'lishi kerak", nameof(amount));

        if (customerId <= 0)
            throw new ArgumentException("Mijoz ID noto'g'ri", nameof(customerId));

        if (orderId <= 0)
            throw new ArgumentException("Buyurtma ID noto'g'ri", nameof(orderId));

        var now = DateTime.UtcNow;
        var nowString = now.ToString("yyyy-MM-dd HH:mm:ss");

        return new CashbackTransaction
        {
            CustomerId = customerId,
            OrderId = orderId,
            Type = CashbackTransactionType.Used,
            Amount = -amount, // Manfiy summa (kamayish)
            RemainingAmount = 0, // Used transaction'da RemainingAmount 0
            EarnedAt = nowString,
            ExpiresAt = now.AddYears(1).ToString("yyyy-MM-dd HH:mm:ss"), // Ahamiyati yo'q
            UsedAt = nowString,
            Description = description ?? $"Used for Order #{orderId}",
            TransactionNumber = GenerateTransactionNumber()
        };
    }

    /// <summary>
    /// Expired cashback yaratish (muddati tugaganda)
    /// </summary>
    /// <param name="customerId">Mijoz ID</param>
    /// <param name="originalOrderId">Qaysi buyurtmadan yig'ilgan edi</param>
    /// <param name="amount">Muddati tugagan summa (musbat)</param>
    /// <param name="description">Izoh</param>
    /// <returns>Yangi CashbackTransaction entity (Expired)</returns>
    public static CashbackTransaction CreateExpired(
        int customerId,
        int originalOrderId,
        decimal amount,
        string? description = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Muddati tugagan summa musbat bo'lishi kerak", nameof(amount));

        if (customerId <= 0)
            throw new ArgumentException("Mijoz ID noto'g'ri", nameof(customerId));

        var now = DateTime.UtcNow;
        var nowString = now.ToString("yyyy-MM-dd HH:mm:ss");

        return new CashbackTransaction
        {
            CustomerId = customerId,
            OrderId = originalOrderId,
            Type = CashbackTransactionType.Expired,
            Amount = -amount, // Manfiy summa (yo'qolgan)
            RemainingAmount = 0, // Expired transaction'da RemainingAmount 0
            EarnedAt = nowString,
            ExpiresAt = nowString,
            Description = description ?? "Cashback expired",
            TransactionNumber = GenerateTransactionNumber()
        };
    }

    // ==================== BUSINESS METHODS ====================

    /// <summary>
    /// Cashback'ni qisman ishlatish (FIFO uchun)
    /// </summary>
    /// <param name="amountToUse">Ishlatmoqchi bo'lgan summa</param>
    /// <exception cref="InvalidOperationException">Faqat Earned cashback'ni ishlatish mumkin</exception>
    public void UsePartially(decimal amountToUse)
    {
        if (Type != CashbackTransactionType.Earned)
            throw new InvalidOperationException("Faqat earned cashback'ni ishlatish mumkin");

        if (RemainingAmount <= 0)
            throw new InvalidOperationException("Bu cashback allaqachon ishlatilgan");

        if (IsExpired)
            throw new InvalidOperationException("Bu cashback muddati tugagan");

        if (amountToUse <= 0)
            throw new ArgumentException("Ishlatilayotgan summa musbat bo'lishi kerak");

        if (amountToUse > RemainingAmount)
            throw new ArgumentException($"Yetarli cashback yo'q. Mavjud: {RemainingAmount}, Talab: {amountToUse}");

        // Qolgan summani kamaytirish
        RemainingAmount -= amountToUse;

        // Agar to'liq ishlatilgan bo'lsa
        if (RemainingAmount == 0)
        {
            UsedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback'ni muddati tugadi deb belgilash
    /// </summary>
    public void MarkAsExpired()
    {
        if (Type != CashbackTransactionType.Earned)
            throw new InvalidOperationException("Faqat earned cashback'ni expire qilish mumkin");

        if (IsUsed)
            throw new InvalidOperationException("Ishlatilgan cashback'ni expire qilib bo'lmaydi");

        // RemainingAmount 0 ga teng bo'lishi kerak
        RemainingAmount = 0;
        Description = (Description ?? "") + " - Muddati tugadi";
        MarkAsUpdated();
    }

    /// <summary>
    /// Muddatgacha qolgan vaqt
    /// </summary>
    /// <returns>TimeSpan yoki null</returns>
    public TimeSpan? GetTimeUntilExpiry()
    {
        if (Type != CashbackTransactionType.Earned || RemainingAmount <= 0)
            return null;

        var expiryDate = DateTime.Parse(ExpiresAt);
        var timeLeft = expiryDate.Subtract(DateTime.UtcNow);
        return timeLeft.TotalSeconds > 0 ? timeLeft : TimeSpan.Zero;
    }

    /// <summary>
    /// Muddatgacha qolgan kunlar
    /// </summary>
    /// <returns>Kunlar soni</returns>
    public int GetDaysUntilExpiry()
    {
        var timeLeft = GetTimeUntilExpiry();
        return timeLeft?.Days ?? 0;
    }

    /// <summary>
    /// Cashback tez orada tugayaptimi (3 kun qolgan)
    /// </summary>
    /// <returns>true agar 3 kun yoki undan kam qolgan bo'lsa</returns>
    public bool IsExpiringSoon()
    {
        return IsAvailableForUse && GetDaysUntilExpiry() <= 3;
    }

    /// <summary>
    /// Izohni yangilash
    /// </summary>
    /// <param name="description">Yangi izoh</param>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback tranzaksiyasini validatsiya qilish
    /// </summary>
    /// <returns>Validation result</returns>
    public Result Validate()
    {
        var errors = new List<string>();

        if (CustomerId <= 0)
            errors.Add("Mijoz ID noto'g'ri");

        if (string.IsNullOrWhiteSpace(TransactionNumber))
            errors.Add("Tranzaksiya raqami bo'sh bo'lishi mumkin emas");

        if (Type == CashbackTransactionType.Earned)
        {
            if (Amount <= 0)
                errors.Add("Earned cashback summasi musbat bo'lishi kerak");

            if (RemainingAmount < 0 || RemainingAmount > Amount)
                errors.Add("RemainingAmount noto'g'ri");

            if (!OrderId.HasValue)
                errors.Add("Earned cashback uchun buyurtma ID kerak");

            // ExpiresAt kelajakda bo'lishi kerak
            if (DateTime.TryParse(ExpiresAt, out var expiryDate))
            {
                if (expiryDate <= DateTime.UtcNow.AddMinutes(-1))
                    errors.Add("Cashback amal qilish muddati kelajakda bo'lishi kerak");
            }
            else
            {
                errors.Add("ExpiresAt noto'g'ri format");
            }
        }
        else if (Type == CashbackTransactionType.Used)
        {
            if (Amount >= 0)
                errors.Add("Used cashback summasi manfiy bo'lishi kerak");

            if (RemainingAmount != 0)
                errors.Add("Used cashback'da RemainingAmount 0 bo'lishi kerak");

            if (string.IsNullOrWhiteSpace(UsedAt))
                errors.Add("Used cashback uchun ishlatilgan sana kerak");
        }
        else if (Type == CashbackTransactionType.Expired)
        {
            if (Amount >= 0)
                errors.Add("Expired cashback summasi manfiy bo'lishi kerak");

            if (RemainingAmount != 0)
                errors.Add("Expired cashback'da RemainingAmount 0 bo'lishi kerak");
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Tranzaksiya ma'lumotlarini to'liq formatda
    /// </summary>
    /// <returns>To'liq tavsif</returns>
    public string GetFullDescription()
    {
        return Type switch
        {
            CashbackTransactionType.Earned =>
                $"Buyurtma #{OrderId} dan {Amount:N2} so'm cashback yig'ildi. Qolgan: {RemainingAmount:N2} so'm",
            CashbackTransactionType.Used =>
                $"Buyurtma #{OrderId} da {Math.Abs(Amount):N2} so'm cashback ishlatildi",
            CashbackTransactionType.Expired =>
                $"Buyurtma #{OrderId} dan {Math.Abs(Amount):N2} so'm cashback muddati tugadi",
            _ => $"{Type}: {Amount:N2} so'm"
        };
    }

    /// <summary>
    /// Tranzaksiya status ma'lumoti
    /// </summary>
    /// <returns>Status text</returns>
    public string GetStatusText()
    {
        return Type switch
        {
            CashbackTransactionType.Earned when IsUsed =>
                "Ishlatilgan",
            CashbackTransactionType.Earned when IsExpired =>
                "Muddati tugagan",
            CashbackTransactionType.Earned when IsExpiringSoon() =>
                $"Tez orada tugaydi ({GetDaysUntilExpiry()} kun)",
            CashbackTransactionType.Earned =>
                $"Faol ({GetDaysUntilExpiry()} kun qoldi, {RemainingAmount:N2} so'm)",
            CashbackTransactionType.Used =>
                "Ishlatilgan",
            CashbackTransactionType.Expired =>
                "Muddati tugagan",
            _ => Type.ToString()
        };
    }

    /// <summary>
    /// Tranzaksiya qisqacha ma'lumoti
    /// </summary>
    /// <returns>Qisqacha text</returns>
    public string GetSummary()
    {
        var typeText = Type switch
        {
            CashbackTransactionType.Earned => "Yig'ildi",
            CashbackTransactionType.Used => "Ishlatildi",
            CashbackTransactionType.Expired => "Muddati tugadi",
            _ => Type.ToString()
        };

        return $"{typeText}: {Math.Abs(Amount):N2} so'm ({TransactionNumber})";
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// DateTime parse helper
    /// </summary>
    private DateTime ParseDateTime(string dateString)
    {
        if (DateTime.TryParse(dateString, out var result))
            return result;

        return DateTime.UtcNow;
    }

    /// <summary>
    /// Clone this transaction (for testing purposes)
    /// </summary>
    /// <returns>Cloned transaction</returns>
    public CashbackTransaction Clone()
    {
        return new CashbackTransaction
        {
            CustomerId = this.CustomerId,
            OrderId = this.OrderId,
            Type = this.Type,
            Amount = this.Amount,
            RemainingAmount = this.RemainingAmount,
            EarnedAt = this.EarnedAt,
            ExpiresAt = this.ExpiresAt,
            UsedAt = this.UsedAt,
            Description = this.Description,
            TransactionNumber = this.TransactionNumber
        };
    }

    /// <summary>
    /// Override ToString for debugging
    /// </summary>
    public override string ToString()
    {
        return $"CashbackTransaction #{TransactionNumber} - {Type}: {Amount:N2} so'm (Customer: {CustomerId})";
    }
}