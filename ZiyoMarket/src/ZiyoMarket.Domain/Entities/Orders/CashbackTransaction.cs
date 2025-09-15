using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Domain.Entities.Orders;

/// <summary>
/// Cashback tranzaksiya entity'si
/// </summary>
public class CashbackTransaction : BaseEntity
{
    /// <summary>
    /// Mijoz ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Buyurtma ID (agar earned bo'lsa)
    /// </summary>
    public int? OrderId { get; set; }

    /// <summary>
    /// Tranzaksiya turi
    /// </summary>
    public CashbackTransactionType TransactionType { get; set; }

    /// <summary>
    /// Cashback summasi
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Amal qilish muddati (30 kun)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Ishlatilgan sana
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Qaysi buyurtmada ishlatilgan
    /// </summary>
    public int? UsedInOrderId { get; set; }

    /// <summary>
    /// Izohlar
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Tranzaksiya raqami
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    // Navigation Properties

    /// <summary>
    /// Mijoz
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Buyurtma (cashback yig'ilgan)
    /// </summary>
    public virtual Order? Order { get; set; }

    /// <summary>
    /// Buyurtma (cashback ishlatilgan)
    /// </summary>
    public virtual Order? UsedInOrder { get; set; }

    // Business Methods

    /// <summary>
    /// Cashback faolmi (muddati tugamaganmi)
    /// </summary>
    public bool IsActive => TransactionType == CashbackTransactionType.Earned &&
                           !UsedAt.HasValue &&
                           ExpiresAt > DateTime.UtcNow &&
                           !IsDeleted;

    /// <summary>
    /// Cashback ishlatilganmi
    /// </summary>
    public bool IsUsed => UsedAt.HasValue;

    /// <summary>
    /// Cashback muddati tugaganmi
    /// </summary>
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow &&
                            TransactionType == CashbackTransactionType.Earned &&
                            !IsUsed;

    /// <summary>
    /// Ishlatish uchun mavjudmi
    /// </summary>
    public bool IsAvailableForUse => IsActive && !IsUsed && !IsExpired;

    /// <summary>
    /// Tranzaksiya raqamini generate qilish
    /// </summary>
    public static string GenerateTransactionNumber()
    {
        var today = DateTime.UtcNow;
        var random = new Random().Next(100000, 999999);
        return $"CB-{today:yyyyMMdd}-{random}";
    }

    /// <summary>
    /// Earned cashback yaratish
    /// </summary>
    public static CashbackTransaction CreateEarned(int customerId, int orderId, decimal amount, string? notes = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Cashback summasi musbat bo'lishi kerak");

        return new CashbackTransaction
        {
            CustomerId = customerId,
            OrderId = orderId,
            TransactionType = CashbackTransactionType.Earned,
            Amount = amount,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // 30 kun amal qiladi
            Notes = notes,
            TransactionNumber = GenerateTransactionNumber()
        };
    }

    /// <summary>
    /// Used cashback yaratish
    /// </summary>
    public static CashbackTransaction CreateUsed(int customerId, int usedInOrderId, decimal amount, string? notes = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Cashback summasi musbat bo'lishi kerak");

        return new CashbackTransaction
        {
            CustomerId = customerId,
            UsedInOrderId = usedInOrderId,
            TransactionType = CashbackTransactionType.Used,
            Amount = -amount, // Manfiy summa (kamayish)
            UsedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow, // Used cashback uchun ahamiyati yo'q
            Notes = notes,
            TransactionNumber = GenerateTransactionNumber()
        };
    }

    /// <summary>
    /// Expired cashback yaratish
    /// </summary>
    public static CashbackTransaction CreateExpired(int customerId, int originalOrderId, decimal amount, string? notes = null)
    {
        return new CashbackTransaction
        {
            CustomerId = customerId,
            OrderId = originalOrderId,
            TransactionType = CashbackTransactionType.Expired,
            Amount = -amount, // Manfiy summa (yo'qolgan)
            ExpiresAt = DateTime.UtcNow,
            Notes = notes ?? "Muddati tugadi",
            TransactionNumber = GenerateTransactionNumber()
        };
    }

    /// <summary>
    /// Cashback'ni ishlatish
    /// </summary>
    public void Use(int usedInOrderId)
    {
        if (TransactionType != CashbackTransactionType.Earned)
            throw new InvalidOperationException("Faqat earned cashback'ni ishlatish mumkin");

        if (IsUsed)
            throw new InvalidOperationException("Bu cashback allaqachon ishlatilgan");

        if (IsExpired)
            throw new InvalidOperationException("Bu cashback muddati tugagan");

        UsedAt = DateTime.UtcNow;
        UsedInOrderId = usedInOrderId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback'ni muddati tugadi deb belgilash
    /// </summary>
    public void MarkAsExpired()
    {
        if (TransactionType != CashbackTransactionType.Earned)
            throw new InvalidOperationException("Faqat earned cashback'ni expire qilish mumkin");

        if (IsUsed)
            throw new InvalidOperationException("Ishlatilgan cashback'ni expire qilib bo'lmaydi");

        TransactionType = CashbackTransactionType.Expired;
        Notes = (Notes ?? "") + " - Muddati tugadi";
        MarkAsUpdated();
    }

    /// <summary>
    /// Muddatgacha qolgan vaqt
    /// </summary>
    public TimeSpan? GetTimeUntilExpiry()
    {
        if (TransactionType != CashbackTransactionType.Earned || IsUsed)
            return null;

        var timeLeft = ExpiresAt.Subtract(DateTime.UtcNow);
        return timeLeft.TotalSeconds > 0 ? timeLeft : TimeSpan.Zero;
    }

    /// <summary>
    /// Muddatgacha qolgan kunlar
    /// </summary>
    public int GetDaysUntilExpiry()
    {
        var timeLeft = GetTimeUntilExpiry();
        return timeLeft?.Days ?? 0;
    }

    /// <summary>
    /// Cashback tez orada tugayaptimi (3 kun qolgan)
    /// </summary>
    public bool IsExpiringSoon()
    {
        return IsAvailableForUse && GetDaysUntilExpiry() <= 3;
    }

    /// <summary>
    /// Izohni yangilash
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback tranzaksiyasini validatsiya qilish
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (CustomerId <= 0)
            errors.Add("Mijoz ID noto'g'ri");

        if (TransactionType == CashbackTransactionType.Earned)
        {
            if (Amount <= 0)
                errors.Add("Earned cashback summasi musbat bo'lishi kerak");

            if (!OrderId.HasValue)
                errors.Add("Earned cashback uchun buyurtma ID kerak");

            if (ExpiresAt <= DateTime.UtcNow.AddMinutes(-1)) // 1 minut tolerance
                errors.Add("Cashback amal qilish muddati kelajakda bo'lishi kerak");
        }
        else if (TransactionType == CashbackTransactionType.Used)
        {
            if (Amount >= 0)
                errors.Add("Used cashback summasi manfiy bo'lishi kerak");

            if (!UsedInOrderId.HasValue)
                errors.Add("Used cashback uchun ishlatilgan buyurtma ID kerak");

            if (!UsedAt.HasValue)
                errors.Add("Used cashback uchun ishlatilgan sana kerak");
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Tranzaksiya ma'lumotlarini to'liq formatda
    /// </summary>
    public string GetDescription()
    {
        return TransactionType switch
        {
            CashbackTransactionType.Earned => $"Buyurtma #{Order?.OrderNumber} dan {Amount:C} cashback yig'ildi",
            CashbackTransactionType.Used => $"Buyurtma #{UsedInOrder?.OrderNumber} da {Math.Abs(Amount):C} cashback ishlatildi",
            CashbackTransactionType.Expired => $"Buyurtma #{Order?.OrderNumber} dan {Math.Abs(Amount):C} cashback muddati tugadi",
            _ => $"{TransactionType}: {Amount:C}"
        };
    }

    /// <summary>
    /// Tranzaksiya status ma'lumoti
    /// </summary>
    public string GetStatusText()
    {
        return TransactionType switch
        {
            CashbackTransactionType.Earned when IsUsed => "Ishlatilgan",
            CashbackTransactionType.Earned when IsExpired => "Muddati tugagan",
            CashbackTransactionType.Earned when IsExpiringSoon() => $"Tez orada tugaydi ({GetDaysUntilExpiry()} kun)",
            CashbackTransactionType.Earned => $"Faol ({GetDaysUntilExpiry()} kun qoldi)",
            CashbackTransactionType.Used => "Ishlatilgan",
            CashbackTransactionType.Expired => "Muddati tugagan",
            _ => TransactionType.ToString()
        };
    }
}