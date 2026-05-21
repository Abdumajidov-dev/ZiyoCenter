namespace ZiyoMarket.Service.Interfaces;

public interface IEskizSmsService
{
    bool IsEnabled { get; }

    /// <summary>
    /// OTP code yuboradi. Enabled=false bo'lsa "0000" ishlatiladi (SMS yuborilmaydi).
    /// </summary>
    Task<string> SendOtpAsync(string phone);

    /// <summary>
    /// OTP kodni tekshiradi. Enabled=false bo'lsa "0000" bilan solishtiradi.
    /// </summary>
    Task<bool> VerifyOtpAsync(string phone, string code);
}
