namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Hisobot davrlari
/// </summary>
public enum ReportPeriod
{
    /// <summary>
    /// Bugun
    /// </summary>
    Today = 1,

    /// <summary>
    /// Kecha
    /// </summary>
    Yesterday = 2,

    /// <summary>
    /// Oxirgi 7 kun
    /// </summary>
    Last7Days = 3,

    /// <summary>
    /// Oxirgi 30 kun
    /// </summary>
    Last30Days = 4,

    /// <summary>
    /// Joriy hafta
    /// </summary>
    ThisWeek = 5,

    /// <summary>
    /// O'tgan hafta
    /// </summary>
    LastWeek = 6,

    /// <summary>
    /// Joriy oy
    /// </summary>
    ThisMonth = 7,

    /// <summary>
    /// O'tgan oy
    /// </summary>
    LastMonth = 8,

    /// <summary>
    /// Joriy yil
    /// </summary>
    ThisYear = 9,

    /// <summary>
    /// Maxsus oraliq (custom)
    /// </summary>
    Custom = 10
}