using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ZiyoMarket.Service.Extensions;

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Convert ISO 8601 string to DateTime
    /// </summary>
    public static DateTime ParseIso8601(this string dateTimeString)
    {
        if (string.IsNullOrWhiteSpace(dateTimeString))
            return DateTime.MinValue;

        if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
            return result;

        return DateTime.MinValue;
    }

    /// <summary>
    /// Validate phone number format
    /// </summary>
    public static bool IsValidPhoneNumber(this string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Uzbekistan phone format: +998XXXXXXXXX or 998XXXXXXXXX
        var pattern = @"^(\+?998)?[0-9]{9}$";
        return Regex.IsMatch(phone, pattern);
    }

    /// <summary>
    /// Validate email format
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    /// <summary>
    /// Truncate string to max length
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Convert to title case
    /// </summary>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    /// <summary>
    /// Remove special characters
    /// </summary>
    public static string RemoveSpecialCharacters(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return Regex.Replace(value, @"[^a-zA-Z0-9\s]", string.Empty);
    }
}

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Convert DateTime to ISO 8601 string
    /// </summary>
    public static string ToIso8601String(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Check if date is today
    /// </summary>
    public static bool IsToday(this DateTime date)
    {
        return date.Date == DateTime.Today;
    }

    /// <summary>
    /// Check if date is in past
    /// </summary>
    public static bool IsPast(this DateTime date)
    {
        return date < DateTime.Now;
    }

    /// <summary>
    /// Check if date is in future
    /// </summary>
    public static bool IsFuture(this DateTime date)
    {
        return date > DateTime.Now;
    }

    /// <summary>
    /// Get age from birthdate
    /// </summary>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }

    /// <summary>
    /// Get start of day
    /// </summary>
    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Get end of day
    /// </summary>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }
}

/// <summary>
/// Extension methods for decimal (money) operations
/// </summary>
public static class MoneyExtensions
{
    /// <summary>
    /// Round to two decimal places
    /// </summary>
    public static decimal RoundToTwoDecimals(this decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Format as currency (UZS)
    /// </summary>
    public static string FormatAsCurrency(this decimal value)
    {
        return $"{value:N0} so'm";
    }

    /// <summary>
    /// Calculate percentage
    /// </summary>
    public static decimal Percentage(this decimal value, decimal percentage)
    {
        return (value * percentage / 100m).RoundToTwoDecimals();
    }

    /// <summary>
    /// Check if zero or negative
    /// </summary>
    public static bool IsZeroOrNegative(this decimal value)
    {
        return value <= 0;
    }

    /// <summary>
    /// Check if positive
    /// </summary>
    public static bool IsPositive(this decimal value)
    {
        return value > 0;
    }
}

/// <summary>
/// Extension methods for IQueryable
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Apply pagination to queryable
    /// </summary>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Order by property name
    /// </summary>
    public static IQueryable<T> OrderByProperty<T>(this IQueryable<T> query, string propertyName, bool ascending = true)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return query;

        var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var prop = System.Linq.Expressions.Expression.Property(param, propertyName);
        var lambda = System.Linq.Expressions.Expression.Lambda(prop, param);

        var methodName = ascending ? "OrderBy" : "OrderByDescending";
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), prop.Type);

        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }
}

/// <summary>
/// Extension methods for collections
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Check if collection is null or empty
    /// </summary>
    public static bool IsNullOrEmpty<T>(this System.Collections.Generic.IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// Split collection into chunks
    /// </summary>
    public static System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>> Chunk<T>(
        this System.Collections.Generic.IEnumerable<T> source, int chunkSize)
    {
        while (source.Any())
        {
            yield return source.Take(chunkSize);
            source = source.Skip(chunkSize);
        }
    }
}
