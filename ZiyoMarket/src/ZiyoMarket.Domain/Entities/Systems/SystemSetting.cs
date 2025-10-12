using System.Text.Json;
using ZiyoMarket.Domain.Common;

namespace ZiyoMarket.Domain.Entities.Systems;

/// <summary>
/// Tizim sozlamalari entity'si
/// </summary>
public class SystemSetting : BaseAuditableEntity
{
    /// <summary>
    /// Sozlama kaliti (unique)
    /// </summary>
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// Sozlama qiymati
    /// </summary>
    public string SettingValue { get; set; } = string.Empty;

    /// <summary>
    /// Tavsif
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Ma'lumot turi
    /// </summary>
    public string DataType { get; set; } = "String"; // String, Number, Boolean, JSON

    /// <summary>
    /// Tahrirlash mumkinmi
    /// </summary>
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// Kategoriya (guruh)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Default qiymat
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Validation pattern (regex)
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    /// Validation xatolik xabari
    /// </summary>
    public string? ValidationMessage { get; set; }

    /// <summary>
    /// Minimal qiymat (raqamlar uchun)
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Maksimal qiymat (raqamlar uchun)
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// Mumkin bo'lgan qiymatlar (dropdown uchun)
    /// </summary>
    public string? AllowedValues { get; set; } // JSON array

    /// <summary>
    /// Tartiblash uchun
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Maxfiy sozlamami (password, token, etc.)
    /// </summary>
    public bool IsSecure { get; set; } = false;

    /// <summary>
    /// Tizimni qayta ishga tushirish kerakmi
    /// </summary>
    public bool RequiresRestart { get; set; } = false;

    // Business Methods

    /// <summary>
    /// String ma'lumot turimi
    /// </summary>
    public bool IsString => DataType.Equals("String", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Raqam ma'lumot turimi
    /// </summary>
    public bool IsNumber => DataType.Equals("Number", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Boolean ma'lumot turimi
    /// </summary>
    public bool IsBoolean => DataType.Equals("Boolean", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// JSON ma'lumot turimi
    /// </summary>
    public bool IsJson => DataType.Equals("JSON", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Qiymatni string sifatida olish
    /// </summary>
    public string GetStringValue()
    {
        return SettingValue;
    }

    /// <summary>
    /// Qiymatni int sifatida olish
    /// </summary>
    public int GetIntValue()
    {
        if (!IsNumber)
            throw new InvalidOperationException($"Setting '{SettingKey}' raqam turi emas");

        return int.TryParse(SettingValue, out var result) ? result : 0;
    }

    /// <summary>
    /// Qiymatni decimal sifatida olish
    /// </summary>
    public decimal GetDecimalValue()
    {
        if (!IsNumber)
            throw new InvalidOperationException($"Setting '{SettingKey}' raqam turi emas");

        return decimal.TryParse(SettingValue, out var result) ? result : 0;
    }

    /// <summary>
    /// Qiymatni bool sifatida olish
    /// </summary>
    public bool GetBoolValue()
    {
        if (!IsBoolean)
            throw new InvalidOperationException($"Setting '{SettingKey}' boolean turi emas");

        return bool.TryParse(SettingValue, out var result) && result;
    }

    /// <summary>
    /// Qiymatni JSON sifatida olish
    /// </summary>
    public T? GetJsonValue<T>() where T : class
    {
        if (!IsJson)
            throw new InvalidOperationException($"Setting '{SettingKey}' JSON turi emas");

        try
        {
            return string.IsNullOrEmpty(SettingValue)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<T>(SettingValue);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// String qiymat o'rnatish
    /// </summary>
    public void SetStringValue(string value)
    {
        if (!IsEditable)
            throw new InvalidOperationException($"Setting '{SettingKey}' o'zgartirib bo'lmaydi");

        ValidateValue(value);
        SettingValue = value;
        MarkAsUpdated();
    }

    /// <summary>
    /// Int qiymat o'rnatish
    /// </summary>
    public void SetIntValue(int value)
    {
        if (!IsNumber)
            throw new InvalidOperationException($"Setting '{SettingKey}' raqam turi emas");

        SetStringValue(value.ToString());
    }

    /// <summary>
    /// Decimal qiymat o'rnatish
    /// </summary>
    public void SetDecimalValue(decimal value)
    {
        if (!IsNumber)
            throw new InvalidOperationException($"Setting '{SettingKey}' raqam turi emas");

        SetStringValue(value.ToString());
    }

    /// <summary>
    /// Bool qiymat o'rnatish
    /// </summary>
    public void SetBoolValue(bool value)
    {
        if (!IsBoolean)
            throw new InvalidOperationException($"Setting '{SettingKey}' boolean turi emas");

        SetStringValue(value.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// JSON qiymat o'rnatish
    /// </summary>
    public void SetJsonValue<T>(T value) where T : class
    {
        if (!IsJson)
            throw new InvalidOperationException($"Setting '{SettingKey}' JSON turi emas");

        var jsonValue = value == null ? "" : System.Text.Json.JsonSerializer.Serialize(value);
        SetStringValue(jsonValue);
    }

    /// <summary>
    /// Default qiymatga qaytarish
    /// </summary>
    public void ResetToDefault()
    {
        if (!IsEditable)
            throw new InvalidOperationException($"Setting '{SettingKey}' o'zgartirib bo'lmaydi");

        if (!string.IsNullOrEmpty(DefaultValue))
        {
            SettingValue = DefaultValue;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Tavsifni yangilash
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Kategoriyani o'rnatish
    /// </summary>
    public void SetCategory(string? category)
    {
        Category = category?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Validation qoidalarini o'rnatish
    /// </summary>
    public void SetValidationRules(string? pattern, string? message, decimal? minValue = null, decimal? maxValue = null)
    {
        ValidationPattern = pattern?.Trim();
        ValidationMessage = message?.Trim();
        MinValue = minValue;
        MaxValue = maxValue;
        MarkAsUpdated();
    }

    /// <summary>
    /// Mumkin bo'lgan qiymatlarni o'rnatish
    /// </summary>
    public void SetAllowedValues(List<string>? allowedValues)
    {
        if (allowedValues == null || !allowedValues.Any())
        {
            AllowedValues = null;
        }
        else
        {
            AllowedValues = System.Text.Json.JsonSerializer.Serialize(allowedValues);
        }
        MarkAsUpdated();
    }

    /// <summary>
    /// Mumkin bo'lgan qiymatlar ro'yxatini olish
    /// </summary>
    public List<string> GetAllowedValuesList()
    {
        if (string.IsNullOrEmpty(AllowedValues))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(AllowedValues) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Maxfiy sozlama sifatida belgilash
    /// </summary>
    public void MarkAsSecure()
    {
        IsSecure = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Restart talab qiluvchi sozlama sifatida belgilash
    /// </summary>
    public void RequireRestart()
    {
        RequiresRestart = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Qiymatni validation qilish
    /// </summary>
    public Result ValidateValue(string value)
    {
        var errors = new List<string>();

        // DataType validation
        if (IsNumber)
        {
            if (!decimal.TryParse(value, out var numValue))
            {
                errors.Add($"'{value}' to'g'ri raqam emas");
            }
            else
            {
                if (MinValue.HasValue && numValue < MinValue.Value)
                    errors.Add($"Qiymat {MinValue.Value} dan kam bo'la olmaydi");

                if (MaxValue.HasValue && numValue > MaxValue.Value)
                    errors.Add($"Qiymat {MaxValue.Value} dan ko'p bo'la olmaydi");
            }
        }
        else if (IsBoolean)
        {
            if (!bool.TryParse(value, out _))
            {
                errors.Add($"'{value}' to'g'ri boolean qiymat emas (true/false)");
            }
        }
        else if (IsJson)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    System.Text.Json.JsonDocument.Parse(value);
                }
                catch
                {
                    errors.Add($"'{value}' to'g'ri JSON format emas");
                }
            }
        }

        // Pattern validation
        if (!string.IsNullOrEmpty(ValidationPattern) && !string.IsNullOrEmpty(value))
        {
            try
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, ValidationPattern))
                {
                    var message = !string.IsNullOrEmpty(ValidationMessage)
                        ? ValidationMessage
                        : $"Qiymat pattern'ga mos kelmaydi: {ValidationPattern}";
                    errors.Add(message);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Validation pattern xatoligi: {ex.Message}");
            }
        }

        // Allowed values validation
        var allowedValues = GetAllowedValuesList();
        if (allowedValues.Any() && !allowedValues.Contains(value))
        {
            errors.Add($"Qiymat ruxsat etilgan qiymatlardan biri bo'lishi kerak: {string.Join(", ", allowedValues)}");
        }

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Sozlama ma'lumotlarini olish (maxfiy ma'lumotlarsiz)
    /// </summary>
    public string GetDisplayValue()
    {
        if (IsSecure && !string.IsNullOrEmpty(SettingValue))
        {
            return "***";
        }

        return SettingValue;
    }

    /// <summary>
    /// Default sozlamalar yaratish
    /// </summary>
    public static List<SystemSetting> CreateDefaultSettings()
    {
        return new List<SystemSetting>
        {
            // App settings
            new() { SettingKey = "App.Name", SettingValue = "ZiyoMarket", Description = "Ilova nomi", Category = "App" },
            new() { SettingKey = "App.Version", SettingValue = "1.0.0", Description = "Ilova versiyasi", Category = "App" },
            new() { SettingKey = "App.Environment", SettingValue = "Development", Description = "Muhit", Category = "App", AllowedValues = """["Development","Staging","Production"]""" },

            // Store settings
            new() { SettingKey = "Store.Name", SettingValue = "Ziyo Kutubxonasi", Description = "Do'kon nomi", Category = "Store" },
            new() { SettingKey = "Store.Address", SettingValue = "Toshkent sh., Chilonzor tumani", Description = "Do'kon manzili", Category = "Store" },
            new() { SettingKey = "Store.Phone", SettingValue = "+998712345678", Description = "Do'kon telefoni", Category = "Store" },
            new() { SettingKey = "Store.Email", SettingValue = "info@ziyomarket.uz", Description = "Do'kon email", Category = "Store" },

            // Cashback settings
            new() { SettingKey = "Cashback.Percentage", SettingValue = "2.0", Description = "Cashback foizi", DataType = "Number", MinValue = 0, MaxValue = 10, Category = "Cashback" },
            new() { SettingKey = "Cashback.ExpiryDays", SettingValue = "30", Description = "Cashback amal qilish muddati (kun)", DataType = "Number", MinValue = 1, MaxValue = 365, Category = "Cashback" },
            new() { SettingKey = "Cashback.MinOrderAmount", SettingValue = "50000", Description = "Cashback uchun minimal buyurtma summasi", DataType = "Number", MinValue = 0, Category = "Cashback" },

            // Order settings
            new() { SettingKey = "Order.AutoCancelHours", SettingValue = "24", Description = "To'lanmagan buyurtmalarni avtomatik bekor qilish (soat)", DataType = "Number", MinValue = 1, MaxValue = 168, Category = "Order" },
            new() { SettingKey = "Order.MaxItemsPerOrder", SettingValue = "50", Description = "Bitta buyurtmada maksimal item'lar soni", DataType = "Number", MinValue = 1, MaxValue = 1000, Category = "Order" },

            // Notification settings
            new() { SettingKey = "Notification.PushEnabled", SettingValue = "true", Description = "Push notification yoqilganmi", DataType = "Boolean", Category = "Notification" },
            new() { SettingKey = "Notification.EmailEnabled", SettingValue = "true", Description = "Email notification yoqilganmi", DataType = "Boolean", Category = "Notification" },
            new() { SettingKey = "Notification.SmsEnabled", SettingValue = "false", Description = "SMS notification yoqilganmi", DataType = "Boolean", Category = "Notification" },

            // Payment settings
            new() { SettingKey = "Payment.CashEnabled", SettingValue = "true", Description = "Naqd to'lov yoqilganmi", DataType = "Boolean", Category = "Payment" },
            new() { SettingKey = "Payment.CardEnabled", SettingValue = "true", Description = "Karta to'lov yoqilganmi", DataType = "Boolean", Category = "Payment" },
            new() { SettingKey = "Payment.Gateway.Url", SettingValue = "", Description = "To'lov gateway URL", Category = "Payment", IsSecure = true },
            new() { SettingKey = "Payment.Gateway.Key", SettingValue = "", Description = "To'lov gateway kaliti", Category = "Payment", IsSecure = true },

            // Security settings
            new() { SettingKey = "Security.JwtSecret", SettingValue = "", Description = "JWT maxfiy kaliti", Category = "Security", IsSecure = true, RequiresRestart = true },
            new() { SettingKey = "Security.TokenExpiryMinutes", SettingValue = "60", Description = "Token amal qilish muddati (daqiqa)", DataType = "Number", MinValue = 5, MaxValue = 1440, Category = "Security" },
            new() { SettingKey = "Security.PasswordMinLength", SettingValue = "6", Description = "Parol minimal uzunligi", DataType = "Number", MinValue = 4, MaxValue = 50, Category = "Security" },

            // File settings
            new() { SettingKey = "File.MaxSizeMB", SettingValue = "10", Description = "Maksimal fayl o'lchami (MB)", DataType = "Number", MinValue = 1, MaxValue = 100, Category = "File" },
            new() { SettingKey = "File.AllowedExtensions", SettingValue = """["jpg","jpeg","png","pdf","docx"]""", Description = "Ruxsat etilgan fayl kengaytmalari", DataType = "JSON", Category = "File" }
        };
    }

    /// <summary>
    /// Sozlama validatsiya
    /// </summary>
    public Result Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SettingKey))
            errors.Add("Setting key bo'sh bo'la olmaydi");

        var allowedDataTypes = new[] { "String", "Number", "Boolean", "JSON" };
        if (!allowedDataTypes.Contains(DataType))
            errors.Add("Noto'g'ri ma'lumot turi");

        if (MinValue.HasValue && MaxValue.HasValue && MinValue.Value > MaxValue.Value)
            errors.Add("Minimal qiymat maksimal qiymatdan katta bo'la olmaydi");

        // Current value validation
        var valueValidation = ValidateValue(SettingValue);
        if (valueValidation.IsFailure)
            errors.AddRange(valueValidation.Errors);

        return errors.Any() ? Result.Failure(errors) : Result.Success();
    }

    /// <summary>
    /// Sozlama ma'lumotlarini to'liq formatda
    /// </summary>
    public override string ToString()
    {
        return $"{SettingKey} = {GetDisplayValue()} ({DataType})";
    }
}