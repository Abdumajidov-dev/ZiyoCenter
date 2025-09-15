using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Domain.Entities.Users;

namespace ZiyoMarket.Domain.Entities.Notifications;

/// <summary>
/// Xabar entity'si
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Foydalanuvchi ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Foydalanuvchi turi
    /// </summary>
    public UserType UserType { get; set; }

    /// <summary>
    /// Xabar turi
    /// </summary>
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// Xabar sarlavhasi
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Xabar matni
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Qo'shimcha ma'lumotlar (JSON)
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// O'qilganmi
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// O'qilgan sana
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Push notification yuborilganmi
    /// </summary>
    public bool IsPushSent { get; set; } = false;

    /// <summary>
    /// Email yuborilganmi
    /// </summary>
    public bool IsEmailSent { get; set; } = false;

    /// <summary>
    /// SMS yuborilganmi
    /// </summary>
    public bool IsSmsSent { get; set; } = false;

    /// <summary>
    /// Push notification yuborilgan sana
    /// </summary>
    public DateTime? PushSentAt { get; set; }

    /// <summary>
    /// Email yuborilgan sana
    /// </summary>
    public DateTime? EmailSentAt { get; set; }

    /// <summary>
    /// SMS yuborilgan sana
    /// </summary>
    public DateTime? SmsSentAt { get; set; }

    /// <summary>
    /// Muhimlik darajasi
    /// </summary>
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

    /// <summary>
    /// Amal qilish muddati
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Action URL (bosilganda qayerga o'tsin)
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Action tugmasi matni
    /// </summary>
    public string? ActionText { get; set; }

    /// <summary>
    /// Rasm URL (agar bor bo'lsa)
    /// </summary>
    public string? ImageUrl { get; set; }

    // Navigation Properties

    /// <summary>
    /// Mijoz (agar UserType.Customer bo'lsa)
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Sotuvchi (agar UserType.Seller bo'lsa)
    /// </summary>
    public virtual Seller? Seller { get; set; }

    /// <summary>
    /// Admin (agar UserType.Admin bo'lsa)
    /// </summary>
    public virtual Admin? Admin { get; set; }

    // Business Methods

    /// <summary>
    /// Yangi xabarmi (1 kun ichida)
    /// </summary>
    public bool IsNew
    {
        get
        {
            if (DateTime.TryParse(CreatedAt, out var created))
            {
                return DateTime.UtcNow.Subtract(created).TotalDays <= 1;
            }
            return false; // noto‘g‘ri format bo‘lsa, yangi emas deb hisoblaymiz
        }
    }


    /// <summary>
    /// Muddati tugaganmi
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Muhim xabarmi
    /// </summary>
    public bool IsImportant => Priority == "High" || Priority == "Critical";

    /// <summary>
    /// Critical xabarmi
    /// </summary>
    public bool IsCritical => Priority == "Critical";

    /// <summary>
    /// Customer uchun notification yaratish
    /// </summary>
    public static Notification CreateForCustomer(
        int customerId,
        NotificationType type,
        string title,
        string message,
        string? data = null,
        string? actionUrl = null,
        string priority = "Normal")
    {
        return new Notification
        {
            UserId = customerId,
            UserType = UserType.Customer,
            NotificationType = type,
            Title = title,
            Message = message,
            Data = data,
            ActionUrl = actionUrl,
            Priority = priority
        };
    }

    /// <summary>
    /// Admin uchun notification yaratish
    /// </summary>
    public static Notification CreateForAdmin(
        int adminId,
        NotificationType type,
        string title,
        string message,
        string? data = null,
        string? actionUrl = null,
        string priority = "Normal")
    {
        return new Notification
        {
            UserId = adminId,
            UserType = UserType.Admin,
            NotificationType = type,
            Title = title,
            Message = message,
            Data = data,
            ActionUrl = actionUrl,
            Priority = priority
        };
    }

    /// <summary>
    /// Seller uchun notification yaratish
    /// </summary>
    public static Notification CreateForSeller(
        int sellerId,
        NotificationType type,
        string title,
        string message,
        string? data = null,
        string? actionUrl = null,
        string priority = "Normal")
    {
        return new Notification
        {
            UserId = sellerId,
            UserType = UserType.Seller,
            NotificationType = type,
            Title = title,
            Message = message,
            Data = data,
            ActionUrl = actionUrl,
            Priority = priority
        };
    }

    /// <summary>
    /// Xabarni o'qilgan deb belgilash
    /// </summary>
    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Xabarni o'qilmagan deb belgilash
    /// </summary>
    public void MarkAsUnread()
    {
        if (IsRead)
        {
            IsRead = false;
            ReadAt = null;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Push notification yuborildi deb belgilash
    /// </summary>
    public void MarkPushSent()
    {
        IsPushSent = true;
        PushSentAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Email yuborildi deb belgilash
    /// </summary>
    public void MarkEmailSent()
    {
        IsEmailSent = true;
        EmailSentAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// SMS yuborildi deb belgilash
    /// </summary>
    public void MarkSmsSent()
    {
        IsSmsSent = true;
        SmsSentAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sarlavhani yangilash
    /// </summary>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Sarlavha bo'sh bo'la olmaydi");

        Title = newTitle.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Xabar matnini yangilash
    /// </summary>
    public void UpdateMessage(string newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
            throw new ArgumentException("Xabar matni bo'sh bo'la olmaydi");

        Message = newMessage.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Muhimlik darajasini o'rnatish
    /// </summary>
    public void SetPriority(string priority)
    {
        var allowedPriorities = new[] { "Low", "Normal", "High", "Critical" };

        if (!allowedPriorities.Contains(priority))
            throw new ArgumentException($"Noto'g'ri muhimlik darajasi: {priority}");

        Priority = priority;
        MarkAsUpdated();
    }

    /// <summary>
    /// Amal qilish muddatini o'rnatish
    /// </summary>
    public void SetExpirationDate(DateTime? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow)
            throw new ArgumentException("Amal qilish muddati kelajakda bo'lishi kerak");

        ExpiresAt = expiresAt;
        MarkAsUpdated();
    }

    /// <summary>
    /// Action ma'lumotlarini o'rnatish
    /// </summary>
    public void SetAction(string? actionUrl, string? actionText)
    {
        ActionUrl = actionUrl?.Trim();
        ActionText = actionText?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Rasm URL'ini o'rnatish
    /// </summary>
    public void SetImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Qo'shimcha ma'lumotlarni o'rnatish
    /// </summary>
    public void SetData(string? data)
    {
        Data = data?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Barcha kanallar orqali yuborilganmi
    /// </summary>
    public bool IsFullySent()
    {
        // Kamida bitta kanal orqali yuborilgan bo'lishi kerak
        return IsPushSent || IsEmailSent || IsSmsSent;
    }

    /// <summary>
    /// Yuborilgan kanallar ro'yxati
    /// </summary>
    public List<string> GetSentChannels()
    {
        var channels = new List<string>();

        if (IsPushSent) channels.Add("Push");
        if (IsEmailSent) channels.Add("Email");
        if (IsSmsSent) channels.Add("SMS");

        return channels;
    }

    /// <summary>
    /// Xabar yoshi (yaratilganidan beri)
    /// </summary>
    public TimeSpan GetAge()
    {
        if (DateTime.TryParse(CreatedAt, out var created))
        {
            return DateTime.UtcNow.Subtract(created);
        }

        return TimeSpan.Zero; // noto‘g‘ri bo‘lsa, 0 qaytaradi
    }


    /// <summary>
    /// Xabar validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (UserId <= 0)
            errors.Add("Foydalanuvchi ID noto'g'ri");

        if (string.IsNullOrWhiteSpace(Title))
            errors.Add("Sarlavha bo'sh bo'la olmaydi");

        if (string.IsNullOrWhiteSpace(Message))
            errors.Add("Xabar matni bo'sh bo'la olmaydi");

        var allowedPriorities = new[] { "Low", "Normal", "High", "Critical" };
        if (!allowedPriorities.Contains(Priority))
            errors.Add("Noto'g'ri muhimlik darajasi");

        if (ExpiresAt.HasValue && DateTime.TryParse(CreatedAt, out var createdAt))
        {
            if (ExpiresAt.Value <= createdAt)
                errors.Add("Amal qilish muddati yaratilgan sanadan keyin bo'lishi kerak");
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Xabar ma'lumotlarini qisqa formatda
    /// </summary>
    public string GetShortDescription()
    {
        var desc = $"{NotificationType}: {Title}";

        if (IsRead)
            desc += " [O'qilgan]";
        else if (IsNew)
            desc += " [Yangi]";

        if (IsImportant)
            desc += $" [{Priority}]";

        return desc;
    }

    /// <summary>
    /// Xabar ma'lumotlarini to'liq formatda
    /// </summary>
    public override string ToString()
    {
        return $"{NotificationType} - {Title} ({UserType} #{UserId})";
    }
}