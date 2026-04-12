using System;
using System.Globalization;

namespace ZiyoMarket.Service.Helpers;

public static class SafeTypeHelper
{
    private static readonly string[] DateFormats = 
    { 
        "yyyy-MM-dd HH:mm:ss", 
        "yyyy-MM-dd", 
        "dd/MM/yyyy HH:mm:ss", 
        "dd.MM.yyyy HH:mm:ss",
        "MM/dd/yyyy HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.fffZ"
    };

    /// <summary>
    /// Safely parses a string date into a DateTime object.
    /// Returns null if parsing fails.
    /// </summary>
    public static DateTime? ToDateTime(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return null;

        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        if (DateTime.TryParse(dateStr, CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
            return result;

        if (DateTime.TryParseExact(dateStr, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            return result;

        return null;
    }

    /// <summary>
    /// Safely parses a string date into a DateTime object with a default value.
    /// </summary>
    public static DateTime ToDateTime(string? dateStr, DateTime defaultValue)
    {
        return ToDateTime(dateStr) ?? defaultValue;
    }

    /// <summary>
    /// Normalizes a DateTime for database string comparison.
    /// </summary>
    public static string ToDbString(this DateTime date)
    {
        return date.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Converts an object to decimal safely.
    /// </summary>
    public static decimal ToDecimal(object? value)
    {
        if (value == null) return 0;
        if (value is decimal d) return d;
        if (value is double db) return (decimal)db;
        if (value is int i) return i;
        
        if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0;
    }
}
