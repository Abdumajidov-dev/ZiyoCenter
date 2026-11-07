using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Service.Services;

public class EskizSmsService : IEskizSmsService
{
    private readonly EskizSettings _settings;
    private readonly OtpSettings _otpSettings;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EskizSmsService> _logger;

    private const string BaseUrl = "https://notify.eskiz.uz";
    private const string CacheKeyPrefix = "otp:";
    private const string VerifiedKeyPrefix = "otp-verified:";
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(5);
    // Tasdiqlangandan keyin ro'yxatdan o'tish formasini to'ldirishga yetadigan vaqt
    private static readonly TimeSpan VerifiedExpiry = TimeSpan.FromMinutes(15);

    public bool IsEnabled => _settings.Enabled;

    public EskizSmsService(
        IOptions<EskizSettings> settings,
        IOptions<OtpSettings> otpSettings,
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        ILogger<EskizSmsService> logger)
    {
        _settings = settings.Value;
        _otpSettings = otpSettings.Value;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> SendOtpAsync(string phone)
    {
        if (!_settings.Enabled)
        {
            // Test mode: hamma uchun 0000
            StoreOtp(phone, _otpSettings.DefaultCode);
            return _otpSettings.DefaultCode;
        }

        var code = GenerateCode();
        var message = $"ZiyoMarket tasdiqlash kodi: {code}. 5 daqiqa ichida foydalaning.";

        var sent = await SendSmsAsync(phone, message);
        if (!sent)
        {
            // Ilgari bu yerda default kod saqlanardi — SMS ishlamay qolsa hamma "1111" bilan kirardi
            _logger.LogWarning("Eskiz SMS failed for {Phone}", phone);
            throw new InvalidOperationException("SMS yuborib bo'lmadi");
        }

        StoreOtp(phone, code);
        _logger.LogInformation("OTP sent to {Phone}", phone);
        return code;
    }

    public Task<bool> VerifyOtpAsync(string phone, string code)
    {
        var key = CacheKeyPrefix + NormalizePhone(phone);
        if (!_cache.TryGetValue(key, out OtpEntry? entry) || entry == null)
        {
            // Kod yuborilmagan bo'lsa default kod faqat test rejimida (SMS o'chiq) qabul qilinadi
            var valid = !_settings.Enabled && code == _otpSettings.DefaultCode;
            if (valid)
                MarkVerified(phone);
            return Task.FromResult(valid);
        }

        if (entry.Code != code)
        {
            // 6 xonali kodni ketma-ket taxmin qilib topishning oldini olish
            if (++entry.FailedAttempts >= MaxFailedAttempts)
                _cache.Remove(key);
            return Task.FromResult(false);
        }

        _cache.Remove(key);
        MarkVerified(phone);
        return Task.FromResult(true);
    }

    public bool ConsumeVerifiedPhone(string phone)
    {
        var key = VerifiedKeyPrefix + NormalizePhone(phone);
        if (!_cache.TryGetValue(key, out _))
            return false;

        _cache.Remove(key);
        return true;
    }

    private void MarkVerified(string phone)
    {
        _cache.Set(VerifiedKeyPrefix + NormalizePhone(phone), true, VerifiedExpiry);
    }

    /// <summary>
    /// "+998 90 123" va "99890123" bir xil kalitga tushishi uchun faqat raqamlar qoldiriladi.
    /// </summary>
    private static string NormalizePhone(string phone)
        => new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());

    private sealed class OtpEntry
    {
        public string Code { get; init; } = string.Empty;
        public int FailedAttempts { get; set; }
    }

    private async Task<bool> SendSmsAsync(string phone, string message)
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            var normalizedPhone = phone.TrimStart('+').Replace(" ", "");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mobile_phone", normalizedPhone),
                new KeyValuePair<string, string>("message", message),
                new KeyValuePair<string, string>("from", _settings.SenderName),
                new KeyValuePair<string, string>("callback_url", "")
            });

            var response = await client.PostAsync($"{BaseUrl}/api/message/sms/send", form);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eskiz SMS send error");
            return false;
        }
    }

    private async Task<string?> GetTokenAsync()
    {
        // Avval settings da token bo'lsa shu ishlatiladi
        if (!string.IsNullOrWhiteSpace(_settings.Token))
            return _settings.Token;

        // Yo'q bo'lsa email/password bilan login qilamiz
        if (string.IsNullOrWhiteSpace(_settings.Email) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogWarning("Eskiz credentials not configured");
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", _settings.Email),
                new KeyValuePair<string, string>("password", _settings.Password)
            });

            var response = await client.PostAsync($"{BaseUrl}/api/auth/login", form);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("data")
                .GetProperty("token")
                .GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eskiz login error");
            return null;
        }
    }

    private void StoreOtp(string phone, string code)
    {
        var key = CacheKeyPrefix + NormalizePhone(phone);
        _cache.Set(key, new OtpEntry { Code = code }, OtpExpiry);
    }

    private static string GenerateCode()
    {
        // OTP taxmin qilinmasligi kerak, shuning uchun kriptografik RNG
        return System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}
