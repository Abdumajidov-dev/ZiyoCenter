using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Service.Services;

public class FcmService : IFcmService
{
    private readonly FcmSettings _settings;
    private readonly ILogger<FcmService> _logger;
    private readonly FirebaseMessaging? _messaging;

    public bool IsEnabled => _settings.Enabled;

    public FcmService(IOptions<FcmSettings> settings, ILogger<FcmService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (_settings.Enabled && !string.IsNullOrWhiteSpace(_settings.CredentialsJson))
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(_settings.CredentialsJson)
                    });
                }
                _messaging = FirebaseMessaging.DefaultInstance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Firebase initialization failed");
            }
        }
    }

    public async Task SendToTokenAsync(string token, string title, string body, Dictionary<string, string>? data = null)
    {
        if (!_settings.Enabled || _messaging == null || string.IsNullOrWhiteSpace(token))
            return;

        try
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification { Title = title, Body = body },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification { Sound = "default" }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Sound = "default", Badge = 1 }
                }
            };

            await _messaging.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FCM token send failed for token: {Token}", token[..Math.Min(10, token.Length)]);
        }
    }

    public async Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null)
    {
        if (!_settings.Enabled || _messaging == null)
            return;

        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification { Title = title, Body = body },
                Data = data,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification { Sound = "default" }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Sound = "default", Badge = 1 }
                }
            };

            await _messaging.SendAsync(message);
            _logger.LogInformation("FCM topic '{Topic}' sent: {Title}", topic, title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FCM topic send failed: {Topic}", topic);
        }
    }

    public async Task SendToMultipleTokensAsync(IEnumerable<string> tokens, string title, string body, Dictionary<string, string>? data = null)
    {
        if (!_settings.Enabled || _messaging == null)
            return;

        var validTokens = tokens.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        if (validTokens.Count == 0)
            return;

        try
        {
            // Firebase bir yo'lda max 500 token qabul qiladi
            foreach (var batch in validTokens.Chunk(500))
            {
                var message = new MulticastMessage
                {
                    Tokens = batch.ToList(),
                    Notification = new Notification { Title = title, Body = body },
                    Data = data,
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification { Sound = "default" }
                    }
                };

                var result = await _messaging.SendEachForMulticastAsync(message);
                _logger.LogInformation("FCM multicast: {Success} success, {Failure} failed",
                    result.SuccessCount, result.FailureCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FCM multicast send failed");
        }
    }
}
