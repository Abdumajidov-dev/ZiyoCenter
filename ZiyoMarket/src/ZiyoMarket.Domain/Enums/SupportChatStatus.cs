namespace ZiyoMarket.Domain.Enums;

/// <summary>
/// Support chat holatlari
/// </summary>
public enum SupportChatStatus
{
    /// <summary>
    /// Ochiq - yangi murojaat
    /// </summary>
    Open = 1,

    /// <summary>
    /// Jarayonda - admin javob bermoqda
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Yopilgan - hal qilindi
    /// </summary>
    Closed = 3,
    Escalated = 4
}