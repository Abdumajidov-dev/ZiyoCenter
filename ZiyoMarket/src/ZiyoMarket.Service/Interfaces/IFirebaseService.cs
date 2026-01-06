namespace ZiyoMarket.Service.Interfaces;

public interface IFirebaseService
{
    /// <summary>
    /// Bitta foydalanuvchiga push notification yuborish
    /// </summary>
    /// <param name="fcmToken">FCM token (Customer.FcmToken)</param>
    /// <param name="title">Notification sarlavhasi</param>
    /// <param name="body">Notification matni</param>
    /// <param name="data">Qo'shimcha ma'lumotlar (optional)</param>
    Task<bool> SendNotificationToUserAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>
    /// Ko'p foydalanuvchilarga push notification yuborish (batch)
    /// </summary>
    /// <param name="fcmTokens">FCM tokenlar ro'yxati</param>
    /// <param name="title">Notification sarlavhasi</param>
    /// <param name="body">Notification matni</param>
    /// <param name="data">Qo'shimcha ma'lumotlar (optional)</param>
    /// <returns>Muvaffaqiyatli yuborilgan notification soni</returns>
    Task<int> SendNotificationToMultipleUsersAsync(List<string> fcmTokens, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>
    /// Topic ga push notification yuborish (barcha subscribe qilganlar uchun)
    /// </summary>
    /// <param name="topic">Topic nomi (masalan: "all_customers", "new_products")</param>
    /// <param name="title">Notification sarlavhasi</param>
    /// <param name="body">Notification matni</param>
    /// <param name="data">Qo'shimcha ma'lumotlar (optional)</param>
    Task<bool> SendNotificationToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>
    /// Foydalanuvchilarni topicga subscribe qilish
    /// </summary>
    Task<bool> SubscribeToTopicAsync(List<string> fcmTokens, string topic);

    /// <summary>
    /// Foydalanuvchilarni topicdan unsubscribe qilish
    /// </summary>
    Task<bool> UnsubscribeFromTopicAsync(List<string> fcmTokens, string topic);
}
