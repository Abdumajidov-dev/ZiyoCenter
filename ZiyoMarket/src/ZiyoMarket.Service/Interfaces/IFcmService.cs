namespace ZiyoMarket.Service.Interfaces;

public interface IFcmService
{
    bool IsEnabled { get; }

    /// <summary>Bitta qurilmaga token orqali jo'natish (personal)</summary>
    Task SendToTokenAsync(string token, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>Topic ga a'zo bo'lgan barcha qurilmalarga jo'natish (broadcast)</summary>
    Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>Bir nechta tokenlarga bir yo'la jo'natish</summary>
    Task SendToMultipleTokensAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null);
}
