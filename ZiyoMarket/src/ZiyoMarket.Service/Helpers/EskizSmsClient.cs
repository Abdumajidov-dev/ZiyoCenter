using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ZiyoMarket.Service.Helpers;

/// <summary>
/// Eskiz.uz SMS API client
/// </summary>
public class EskizSmsClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EskizSmsClient> _logger;
    private string? _authToken;
    private DateTime? _tokenExpiry;

    public EskizSmsClient(HttpClient httpClient, IConfiguration configuration, ILogger<EskizSmsClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["EskizSms:BaseUrl"] ?? "https://notify.eskiz.uz/api";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    /// <summary>
    /// Eskiz.uz ga autentifikatsiya qilish
    /// </summary>
    private async Task<bool> AuthenticateAsync()
    {
        try
        {
            // Token hali amal qilayotgan bo'lsa, qayta autentifikatsiya qilmaslik
            if (_authToken != null && _tokenExpiry.HasValue && _tokenExpiry.Value > DateTime.UtcNow)
            {
                return true;
            }

            var email = _configuration["EskizSms:Email"];
            var password = _configuration["EskizSms:Password"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.LogError("Eskiz.uz credentials not configured");
                return false;
            }

            var loginData = new
            {
                email = email,
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync("auth/login", loginData);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Eskiz.uz authentication failed: {StatusCode}", response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<EskizAuthResponse>();

            if (result?.Data?.Token != null)
            {
                _authToken = result.Data.Token;
                _tokenExpiry = DateTime.UtcNow.AddDays(29); // Token 30 kun amal qiladi

                _logger.LogInformation("Eskiz.uz authentication successful");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Eskiz.uz authentication");
            return false;
        }
    }

    /// <summary>
    /// SMS yuborish
    /// </summary>
    public async Task<EskizSmsResponse> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // Autentifikatsiya
            if (!await AuthenticateAsync())
            {
                return new EskizSmsResponse
                {
                    Success = false,
                    Message = "Authentication failed"
                };
            }

            // Telefon raqamini tozalash (faqat raqamlar)
            var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");

            var smsData = new
            {
                mobile_phone = cleanPhone,
                message = message,
                from = "4546", // Eskiz.uz default sender
                callback_url = _configuration["EskizSms:CallbackUrl"] ?? ""
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "message/sms/send");
            request.Headers.Add("Authorization", $"Bearer {_authToken}");
            request.Content = JsonContent.Create(smsData);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<EskizSendSmsResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Status == "success")
                {
                    return new EskizSmsResponse
                    {
                        Success = true,
                        Message = "SMS sent successfully",
                        MessageId = result.Data?.MessageId?.ToString()
                    };
                }
            }

            _logger.LogWarning("SMS send failed: {Response}", responseContent);

            return new EskizSmsResponse
            {
                Success = false,
                Message = $"Failed to send SMS: {responseContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return new EskizSmsResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// SMS holatini tekshirish
    /// </summary>
    public async Task<string?> GetSmsStatusAsync(string messageId)
    {
        try
        {
            if (!await AuthenticateAsync())
            {
                return null;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"message/sms/status/{messageId}");
            request.Headers.Add("Authorization", $"Bearer {_authToken}");

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<EskizStatusResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data?.Status;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SMS status for message {MessageId}", messageId);
            return null;
        }
    }
}

// Response models
public class EskizAuthResponse
{
    public EskizAuthData? Data { get; set; }
}

public class EskizAuthData
{
    public string? Token { get; set; }
}

public class EskizSendSmsResponse
{
    public string? Status { get; set; }
    public EskizSmsData? Data { get; set; }
}

public class EskizSmsData
{
    public int? MessageId { get; set; }
}

public class EskizStatusResponse
{
    public EskizStatusData? Data { get; set; }
}

public class EskizStatusData
{
    public string? Status { get; set; }
}

public class EskizSmsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageId { get; set; }
}
