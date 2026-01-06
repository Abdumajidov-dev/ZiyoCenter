using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Service.Services;

public class FirebaseService : IFirebaseService
{
    private readonly ILogger<FirebaseService> _logger;
    private readonly FirebaseApp _firebaseApp;

    public FirebaseService(ILogger<FirebaseService> logger)
    {
        _logger = logger;

        // Initialize Firebase Admin SDK
        if (FirebaseApp.DefaultInstance == null)
        {
            var serviceAccountPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-service-account.json");

            if (!File.Exists(serviceAccountPath))
            {
                _logger.LogError("Firebase service account file not found at: {Path}", serviceAccountPath);
                throw new FileNotFoundException("Firebase service account file not found", serviceAccountPath);
            }

            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(serviceAccountPath)
            });

            _logger.LogInformation("Firebase Admin SDK initialized successfully");
        }
        else
        {
            _firebaseApp = FirebaseApp.DefaultInstance;
        }
    }

    /// <summary>
    /// Bitta foydalanuvchiga push notification yuborish
    /// </summary>
    public async Task<bool> SendNotificationToUserAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
            {
                _logger.LogWarning("FCM token is empty, cannot send notification");
                return false;
            }

            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "ziyomarket_notifications",
                        Sound = "default",
                        Priority = NotificationPriority.HIGH
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = title,
                            Body = body
                        },
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent notification to {Token}. Response: {Response}", fcmToken, response);
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex, "Firebase messaging error while sending notification to {Token}", fcmToken);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to {Token}", fcmToken);
            return false;
        }
    }

    /// <summary>
    /// Ko'p foydalanuvchilarga push notification yuborish (batch)
    /// </summary>
    public async Task<int> SendNotificationToMultipleUsersAsync(List<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null)
    {
        if (fcmTokens == null || fcmTokens.Count == 0)
        {
            _logger.LogWarning("FCM tokens list is empty");
            return 0;
        }

        // Remove null/empty tokens
        fcmTokens = fcmTokens.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

        if (fcmTokens.Count == 0)
        {
            return 0;
        }

        try
        {
            var message = new MulticastMessage
            {
                Tokens = fcmTokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "ziyomarket_notifications",
                        Sound = "default",
                        Priority = NotificationPriority.HIGH
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = title,
                            Body = body
                        },
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            _logger.LogInformation("Sent notifications to {SuccessCount}/{TotalCount} users", response.SuccessCount, fcmTokens.Count);

            if (response.FailureCount > 0)
            {
                _logger.LogWarning("Failed to send {FailureCount} notifications", response.FailureCount);
            }

            return response.SuccessCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch notifications");
            return 0;
        }
    }

    /// <summary>
    /// Topic ga push notification yuborish (barcha subscribe qilganlar uchun)
    /// </summary>
    public async Task<bool> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null)
    {
        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "ziyomarket_notifications",
                        Sound = "default",
                        Priority = NotificationPriority.HIGH
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = title,
                            Body = body
                        },
                        Sound = "default"
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent notification to topic '{Topic}'. Response: {Response}", topic, response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to topic '{Topic}'", topic);
            return false;
        }
    }

    /// <summary>
    /// Foydalanuvchilarni topicga subscribe qilish
    /// </summary>
    public async Task<bool> SubscribeToTopicAsync(List<string> fcmTokens, string topic)
    {
        try
        {
            fcmTokens = fcmTokens.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

            if (fcmTokens.Count == 0)
                return false;

            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(fcmTokens, topic);
            _logger.LogInformation("Subscribed {SuccessCount}/{TotalCount} users to topic '{Topic}'",
                response.SuccessCount, fcmTokens.Count, topic);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing users to topic '{Topic}'", topic);
            return false;
        }
    }

    /// <summary>
    /// Foydalanuvchilarni topicdan unsubscribe qilish
    /// </summary>
    public async Task<bool> UnsubscribeFromTopicAsync(List<string> fcmTokens, string topic)
    {
        try
        {
            fcmTokens = fcmTokens.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

            if (fcmTokens.Count == 0)
                return false;

            var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(fcmTokens, topic);
            _logger.LogInformation("Unsubscribed {SuccessCount}/{TotalCount} users from topic '{Topic}'",
                response.SuccessCount, fcmTokens.Count, topic);

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing users from topic '{Topic}'", topic);
            return false;
        }
    }
}
