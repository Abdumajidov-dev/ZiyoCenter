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
    private static readonly TimeSpan OtpExpiry = TimeSpan.FromMinutes(5);

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
        if (sent)
        {
            StoreOtp(phone, code);
            _logger.LogInformation("OTP sent to {Phone}", phone);
        }
        else
        {
            _logger.LogWarning("Eskiz SMS failed for {Phone}, falling back to default code", phone);
            StoreOtp(phone, _otpSettings.DefaultCode);
        }

        return code;
    }

    public Task<bool> VerifyOtpAsync(string phone, string code)
    {
        var key = CacheKeyPrefix + phone;
        if (_cache.TryGetValue(key, out string? stored))
        {
            var valid = stored == code;
            if (valid)
                _cache.Remove(key);
            return Task.FromResult(valid);
        }

        // Cache da yo'q — test mode uchun default code ni tekshir
        return Task.FromResult(code == _otpSettings.DefaultCode);
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
        var key = CacheKeyPrefix + phone;
        _cache.Set(key, code, OtpExpiry);
    }

    private static string GenerateCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
