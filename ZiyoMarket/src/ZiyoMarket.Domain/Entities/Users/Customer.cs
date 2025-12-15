using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Support;

namespace ZiyoMarket.Domain.Entities.Users;

/// <summary>
/// Mijoz entity'si
/// </summary>
public class Customer : BaseAuditableEntity
{
    /// <summary>
    /// Ism
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Familya
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Telefon raqami (unique)
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Parol hash'i
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Manzil
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// FCM token (push notification uchun)
    /// </summary>
    public string? FcmToken { get; set; }

    /// <summary>
    /// Cashback balansi
    /// </summary>
    public decimal CashbackBalance { get; set; } = 0;

    /// <summary>
    /// Faolmi
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Mijozning buyurtmalari
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>
    /// Mijozning savat item'lari
    /// </summary>
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    /// <summary>
    /// Mijozning yoqtirgan mahsulotlari
    /// </summary>
    public virtual ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();

    /// <summary>
    /// Mijozning cashback tranzaksiyalari
    /// </summary>
    public virtual ICollection<CashbackTransaction> CashbackTransactions { get; set; } = new List<CashbackTransaction>();

    /// <summary>
    /// Mijozning support chat'lari
    /// </summary>
    public virtual ICollection<SupportChat> SupportChats { get; set; } = new List<SupportChat>();

    /// <summary>
    /// Mijozning xabarlari
    /// </summary>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    // Business Methods

    /// <summary>
    /// To'liq ismi
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Cashback qo'shish
    /// </summary>
    public void AddCashback(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Cashback summasi musbat bo'lishi kerak");

        CashbackBalance += amount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Cashback ishlatish
    /// </summary>
    public void UseCashback(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Ishlatilayotgan summa musbat bo'lishi kerak");

        if (amount > CashbackBalance)
            throw new InvalidOperationException("Cashback balansi yetarli emas");

        CashbackBalance -= amount;
        MarkAsUpdated();
    }

    /// <summary>
    /// Mijozni faollashtirish
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Mijozni faolsizlashtirish
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// FCM token yangilash
    /// </summary>
    public void UpdateFcmToken(string? fcmToken)
    {
        FcmToken = fcmToken;
        MarkAsUpdated();
    }
}