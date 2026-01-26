using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        var baseUrl = _configuration["EskizSms:BaseUrl"] ?? "https://notify.eskiz.uz/api/";
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        
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

            // Eskiz often expects form-data for login despite docs saying JSON sometimes, but let's try KeyValuePair to be safe as per standard PHP examples they provide
            var loginData = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password }
            };

            // Using FormUrlEncodedContent is safer for PHP backends usually
            using var content = new FormUrlEncodedContent(loginData);
            var response = await _httpClient.PostAsync("auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Eskiz.uz authentication failed: {StatusCode}, Body: {Body}", response.StatusCode, errorBody);
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

            _logger.LogError("Eskiz.uz auth response invalid: Token missing");
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
            var cleanPhone = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "").Trim();

            // Form data for sending SMS
            var smsData = new Dictionary<string, string>
            {
                { "mobile_phone", cleanPhone },
                { "message", message },
                { "from", "4546" }, // Default sender
                { "callback_url", _configuration["EskizSms:CallbackUrl"] ?? "" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "message/sms/send");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            request.Content = new FormUrlEncodedContent(smsData);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Parse success response
                // Success: {"id":"...","message":"Waiting for SMS provider","status":"waiting"}
                try 
                {
                   var result = JsonSerializer.Deserialize<EskizSendSmsResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                   
                   // "waiting" status is considered success for submission
                   if (result?.Status == "waiting" || result?.Status == "success")
                   {
                       return new EskizSmsResponse
                       {
                           Success = true,
                           Message = "SMS sent successfully",
                           MessageId = result.Id
                       };
                   }
                } 
                catch 
                {
                    // Fallback if JSON parsing fails but status was 200
                    _logger.LogWarning("Failed to parse Eskiz success response: {Content}", responseContent);
                }
            }

            _logger.LogWarning("SMS send failed: {StatusCode} - {Response}", response.StatusCode, responseContent);

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
}

// Response models
public class EskizAuthResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public EskizAuthData? Data { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
}

public class EskizAuthData
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

public class EskizSendSmsResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; } // ID is string "59bf..."

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class EskizSmsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageId { get; set; }
}
