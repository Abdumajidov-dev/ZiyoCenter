using ZiyoMarket.Domain.Common;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Domain.Entities.Support;

/// <summary>
/// Support chat xabari entity'si
/// </summary>
public class SupportMessage : BaseEntity
{
    /// <summary>
    /// Chat ID
    /// </summary>
    public int ChatId { get; set; }

    /// <summary>
    /// Jo'natuvchi ID (customer yoki admin)
    /// </summary>
    public int SenderId { get; set; }

    /// <summary>
    /// Jo'natuvchi turi
    /// </summary>
    public UserType SenderType { get; set; }

    /// <summary>
    /// Xabar matni
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Xabar turi
    /// </summary>
    public string MessageType { get; set; } = "Text"; // Text, Image, File, System

    /// <summary>
    /// Fayl URL (agar fayl yuborilgan bo'lsa)
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// Fayl nomi
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Fayl o'lchami (bytes)
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// O'qilganmi
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// O'qilgan sana
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// O'chirilganmi (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Tahrirlanganmi
    /// </summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>
    /// Tahrirlanган sana
    /// </summary>
    public DateTime? EditedAt { get; set; }

    /// <summary>
    /// Asl xabar (tahrirlangan bo'lsa)
    /// </summary>
    public string? OriginalMessage { get; set; }

    /// <summary>
    /// Reply qilingan xabar ID
    /// </summary>
    public int? ReplyToMessageId { get; set; }

    /// <summary>
    /// Internal xabarmi (admin'lar o'rtasida)
    /// </summary>
    public bool IsInternal { get; set; } = false;

    /// <summary>
    /// Metadata (JSON format)
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation Properties

    /// <summary>
    /// Chat
    /// </summary>
    public virtual SupportChat Chat { get; set; } = null!;

    /// <summary>
    /// Reply qilingan xabar
    /// </summary>
    public virtual SupportMessage? ReplyToMessage { get; set; }

    /// <summary>
    /// Bu xabarga reply'lar
    /// </summary>
    public virtual ICollection<SupportMessage> Replies { get; set; } = new List<SupportMessage>();

    // Business Methods

    /// <summary>
    /// Mijoz xabarimi
    /// </summary>
    public bool IsCustomerMessage => SenderType == UserType.Customer;

    /// <summary>
    /// Admin xabarimi
    /// </summary>
    public bool IsAdminMessage => SenderType == UserType.Admin;

    /// <summary>
    /// Tizim xabarimi
    /// </summary>
    public bool IsSystemMessage => MessageType == "System";

    /// <summary>
    /// Fayl xabarimi
    /// </summary>
    public bool IsFileMessage => MessageType == "File" || MessageType == "Image";

    /// <summary>
    /// Rasm xabarimi
    /// </summary>
    public bool IsImageMessage => MessageType == "Image";

    /// <summary>
    /// Reply xabarimi
    /// </summary>
    public bool IsReply => ReplyToMessageId.HasValue;

    /// <summary>
    /// Yangi xabarmi (5 daqiqa ichida)
    /// </summary>
    public bool IsRecent
    {
        get
        {
            if (DateTime.TryParse(CreatedAt, out var createdAt))
            {
                return DateTime.UtcNow.Subtract(createdAt).TotalMinutes <= 5;
            }
            return false; // noto‘g‘ri format bo‘lsa
        }
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
    /// Xabarni tahrirlash
    /// </summary>
    public void EditMessage(string newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
            throw new ArgumentException("Xabar matni bo'sh bo'la olmaydi");

        if (IsSystemMessage)
            throw new InvalidOperationException("Tizim xabarlarini tahrirlash mumkin emas");

        if (IsFileMessage)
            throw new InvalidOperationException("Fayl xabarlarini tahrirlash mumkin emas");

        // Agar birinchi marta tahrirlanayotgan bo'lsa, asl matnni saqlash
        if (!IsEdited)
        {
            OriginalMessage = Message;
        }

        Message = newMessage.Trim();
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    /// <summary>
    /// Xabarni soft delete qilish
    /// </summary>
    public void DeleteMessage()
    {
        IsDeleted = true;
        Message = "[Bu xabar o'chirilgan]";
        MarkAsUpdated();
    }

    /// <summary>
    /// Xabarni tiklash
    /// </summary>
    public void RestoreMessage()
    {
        if (IsDeleted)
        {
            IsDeleted = false;

            // Agar asl matn saqlab qo'yilgan bo'lsa, uni qaytarish
            if (!string.IsNullOrEmpty(OriginalMessage))
            {
                Message = OriginalMessage;
            }

            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Fayl ma'lumotlarini o'rnatish
    /// </summary>
    public void SetFileInfo(string fileUrl, string fileName, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("Fayl URL bo'sh bo'la olmaydi");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Fayl nomi bo'sh bo'la olmaydi");

        if (fileSize <= 0)
            throw new ArgumentException("Fayl o'lchami musbat bo'lishi kerak");

        FileUrl = fileUrl.Trim();
        FileName = fileName.Trim();
        FileSize = fileSize;

        // Fayl turiga qarab MessageType o'rnatish
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        MessageType = imageExtensions.Contains(extension) ? "Image" : "File";

        MarkAsUpdated();
    }

    /// <summary>
    /// Reply o'rnatish
    /// </summary>
    public void SetReplyTo(int replyToMessageId)
    {
        if (replyToMessageId <= 0)
            throw new ArgumentException("Reply message ID noto'g'ri");

        ReplyToMessageId = replyToMessageId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Metadata o'rnatish
    /// </summary>
    public void SetMetadata(string? metadata)
    {
        Metadata = metadata?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Internal xabar deb belgilash
    /// </summary>
    public void MarkAsInternal()
    {
        IsInternal = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Fayl o'lchamini human-readable formatda
    /// </summary>
    public string GetFileSizeFormatted()
    {
        if (!FileSize.HasValue)
            return "";

        var size = FileSize.Value;

        if (size < 1024)
            return $"{size} B";

        if (size < 1024 * 1024)
            return $"{size / 1024:F1} KB";

        if (size < 1024 * 1024 * 1024)
            return $"{size / (1024 * 1024):F1} MB";

        return $"{size / (1024 * 1024 * 1024):F1} GB";
    }

    /// <summary>
    /// Xabar yoshi
    /// </summary>
    public TimeSpan? GetAge()
    {
        if (DateTime.TryParse(CreatedAt, out var createdAt))
        {
            return DateTime.UtcNow.Subtract(createdAt);
        }
        return null; // noto‘g‘ri format bo‘lsa
    }


    /// <summary>
    /// Qisqa xabar matni (preview uchun)
    /// </summary>
    public string GetPreviewText(int maxLength = 100)
    {
        if (IsDeleted)
            return "[O'chirilgan xabar]";

        if (IsFileMessage)
        {
            var fileInfo = !string.IsNullOrEmpty(FileName) ? FileName : "Fayl";
            return IsImageMessage ? $"🖼️ Rasm: {fileInfo}" : $"📎 Fayl: {fileInfo}";
        }

        if (string.IsNullOrEmpty(Message))
            return "[Bo'sh xabar]";

        return Message.Length <= maxLength
            ? Message
            : Message.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Xabar statusini olish
    /// </summary>
    public string GetStatusText()
    {
        var status = new List<string>();

        if (IsDeleted)
            status.Add("O'chirilgan");
        else if (IsEdited)
            status.Add("Tahrirlangan");

        if (IsRead)
            status.Add("O'qilgan");
        else
            status.Add("O'qilmagan");

        if (IsInternal)
            status.Add("Ichki");

        return string.Join(", ", status);
    }

    /// <summary>
    /// Xabar validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (ChatId <= 0)
            errors.Add("Chat ID noto'g'ri");

        if (SenderId <= 0)
            errors.Add("Jo'natuvchi ID noto'g'ri");

        if (string.IsNullOrWhiteSpace(Message) && !IsFileMessage)
            errors.Add("Xabar matni yoki fayl bo'lishi kerak");

        var allowedMessageTypes = new[] { "Text", "Image", "File", "System" };
        if (!allowedMessageTypes.Contains(MessageType))
            errors.Add("Noto'g'ri xabar turi");

        if (IsFileMessage)
        {
            if (string.IsNullOrWhiteSpace(FileUrl))
                errors.Add("Fayl xabari uchun fayl URL kerak");

            if (string.IsNullOrWhiteSpace(FileName))
                errors.Add("Fayl xabari uchun fayl nomi kerak");

            if (!FileSize.HasValue || FileSize <= 0)
                errors.Add("Fayl xabari uchun fayl o'lchami kerak");
        }

        if (ReplyToMessageId.HasValue && ReplyToMessageId <= 0)
            errors.Add("Reply message ID noto'g'ri");

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// System xabar yaratish
    /// </summary>
    public static SupportMessage CreateSystemMessage(int chatId, string message)
    {
        return new SupportMessage
        {
            ChatId = chatId,
            SenderId = 0, // System sender
            SenderType = UserType.Admin, // System messages are admin type
            Message = message,
            MessageType = "System",
            IsRead = true, // System messages are auto-read
            ReadAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Fayl xabar yaratish
    /// </summary>
    public static SupportMessage CreateFileMessage(
        int chatId,
        int senderId,
        UserType senderType,
        string fileUrl,
        string fileName,
        long fileSize,
        string? message = null)
    {
        var supportMessage = new SupportMessage
        {
            ChatId = chatId,
            SenderId = senderId,
            SenderType = senderType,
            Message = message ?? $"Fayl yuborildi: {fileName}",
            FileUrl = fileUrl,
            FileName = fileName,
            FileSize = fileSize
        };

        // Fayl turiga qarab MessageType o'rnatish
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        supportMessage.MessageType = imageExtensions.Contains(extension) ? "Image" : "File";

        return supportMessage;
    }

    /// <summary>
    /// Xabar ma'lumotlarini to'liq formatda
    /// </summary>
    public override string ToString()
    {
        var senderInfo = $"{SenderType} #{SenderId}";
        var messagePreview = GetPreviewText(50);

        return $"{senderInfo}: {messagePreview} ({CreatedAt:HH:mm})";
    }
}