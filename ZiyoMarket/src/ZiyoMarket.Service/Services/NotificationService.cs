using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Notifications;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFirebaseService _firebaseService;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, IFirebaseService firebaseService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _firebaseService = firebaseService;
    }

    public async Task<Result> SendNotificationAsync(CreateNotificationDto request)
    {
        try
        {
            var notification = new Notification
            {
                Title = request.Title,
                Message = request.Message,
                NotificationType = Enum.Parse<NotificationType>(request.Type),
                Priority = request.Priority,
                UserId = request.UserId,
                UserType = Enum.Parse<UserType>(request.UserType),
                ActionUrl = request.ActionUrl,
                ActionText = request.ActionText,
                ImageUrl = request.ImageUrl,
                Data = request.Data
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Firebase push notification yuborish
            if (request.SendPushNotification && !string.IsNullOrEmpty(request.FcmToken))
            {
                var data = new Dictionary<string, string>
                {
                    { "notification_id", notification.Id.ToString() },
                    { "type", request.Type },
                    { "action_url", request.ActionUrl ?? "" }
                };

                await _firebaseService.SendNotificationToUserAsync(
                    request.FcmToken,
                    request.Title,
                    request.Message,
                    data
                );
            }

            return Result.Success("Notification sent successfully");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> SendBulkNotificationAsync(List<CreateNotificationDto> requests)
    {
        try
        {
            var notifications = requests.Select(r => new Notification
            {
                Title = r.Title,
                Message = r.Message,
                NotificationType = Enum.Parse<NotificationType>(r.Type),
                Priority = r.Priority,
                UserId = r.UserId,
                UserType = Enum.Parse<UserType>(r.UserType),
                ActionUrl = r.ActionUrl,
                ActionText = r.ActionText,
                ImageUrl = r.ImageUrl,
                Data = r.Data
            }).ToList();

            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success($"{notifications.Count} notifications sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<NotificationDto>>> GetNotificationsAsync(int userId, string userType, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var skip = (pageNumber - 1) * pageSize;
            var query = _unitOfWork.Notifications.Table
                .Where(n => n.UserId == userId &&
                    n.UserType.ToString() == userType &&
                    n.DeletedAt == null)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize);

            var notifications = await query.ToListAsync();
            var dtos = _mapper.Map<List<NotificationDto>>(notifications);

            return Result<List<NotificationDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<NotificationDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetUnreadCountAsync(int userId, string userType)
    {
        try
        {
            var count = await _unitOfWork.Notifications.Table
                .CountAsync(n => n.UserId == userId &&
                n.UserType.ToString() == userType &&
                !n.IsRead &&
                n.DeletedAt == null);

            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            return Result<int>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> MarkAsReadAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                return Result.NotFound("Notification not found");

            if (notification.UserId != userId)
                return Result.Forbidden("Access denied");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.MarkAsUpdated();

            await _unitOfWork.Notifications.Update(notification, notificationId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Notification marked as read");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> MarkAllAsReadAsync(int userId, string userType)
    {
        try
        {
            var notifications = await _unitOfWork.Notifications.Table
                .Where(n => n.UserId == userId &&
                    n.UserType.ToString() == userType &&
                    !n.IsRead &&
                    n.DeletedAt == null)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.MarkAsUpdated();
                await _unitOfWork.Notifications.Update(notification, notification.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{notifications.Count} notifications marked as read");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteNotificationAsync(int notificationId, int userId)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification == null)
                return Result.NotFound("Notification not found");

            if (notification.UserId != userId)
                return Result.Forbidden("Access denied");

            notification.Delete();
            await _unitOfWork.Notifications.Update(notification, notificationId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Notification deleted");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> NotifyOrderCreatedAsync(int customerId, int orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return Result.NotFound("Order not found");

            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null)
                return Result.NotFound("Customer not found");

            var notification = new Notification
            {
                Title = "Yangi buyurtma",
                Message = $"Sizning #{order.OrderNumber} raqamli buyurtmangiz qabul qilindi",
                NotificationType = NotificationType.OrderCreated,
                Priority = "Normal",
                UserId = customerId,
                UserType = UserType.Customer,
                ActionUrl = $"/orders/{orderId}",
                ActionText = "Buyurtmani ko'rish",
                Data = orderId.ToString()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // ✅ Firebase push notification yuborish
            if (!string.IsNullOrEmpty(customer.FcmToken))
            {
                var data = new Dictionary<string, string>
                {
                    { "notification_id", notification.Id.ToString() },
                    { "order_id", orderId.ToString() },
                    { "order_number", order.OrderNumber },
                    { "type", "order_created" }
                };

                await _firebaseService.SendNotificationToUserAsync(
                    customer.FcmToken,
                    notification.Title,
                    notification.Message,
                    data
                );
            }

            return Result.Success("Order creation notification sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> NotifyOrderStatusChangedAsync(int orderId, string newStatus)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return Result.NotFound("Order not found");

            var notification = new Notification
            {
                Title = "Buyurtma holati o'zgarishi",
                Message = $"#{order.OrderNumber} raqamli buyurtmangiz holati: {newStatus}",
                NotificationType = NotificationType.OrderStatusChanged,
                Priority = "Normal",
                UserId = order.CustomerId,
                UserType = UserType.Customer,
                ActionUrl = $"/orders/{orderId}",
                ActionText = "Buyurtmani ko'rish",
                Data = orderId.ToString()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Order status change notification sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> NotifyCashbackEarnedAsync(int customerId, decimal amount, int orderId)
    {
        try
        {
            var notification = new Notification
            {
                Title = "Cashback qo'shildi",
                Message = $"Siz {amount:N0} so'm miqdorida cashback qo'shdingiz",
                NotificationType = NotificationType.CashbackEarned,
                Priority = "Normal",
                UserId = customerId,
                UserType = UserType.Customer,
                ActionUrl = "/profile/cashback",
                ActionText = "Cashback balansini ko'rish",
                Data = orderId.ToString()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Cashback earned notification sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> NotifyAdminAboutNewOrderAsync(int orderId)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                return Result.NotFound("Order not found");

            // Get all active admins
            var admins = await _unitOfWork.Admins
                .SelectAll(a => a.IsActive && a.DeletedAt == null)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    Title = "Yangi buyurtma",
                    Message = $"Yangi #{order.OrderNumber} raqamli buyurtma qabul qilindi",
                    NotificationType = NotificationType.NewOrder,
                    Priority = "High",
                    UserId = admin.Id,
                    UserType = UserType.Admin,
                    ActionUrl = $"/admin/orders/{orderId}",
                    ActionText = "Buyurtmani ko'rish",
                    Data = orderId.ToString()
                };

                await _unitOfWork.Notifications.InsertAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success("Admin notifications sent");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAllNotificationsAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _unitOfWork.Notifications.Table.Where(n => n.DeletedAt == null);

            if (startDate.HasValue)
                query = query.Where(n => DateTime.Parse(n.CreatedAt) >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(n => DateTime.Parse(n.CreatedAt) <= endDate.Value);

            var notifications = await query.ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Delete();
                await _unitOfWork.Notifications.Update(notification, notification.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{notifications.Count} notifications deleted");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<NotificationDto>>> SeedMockNotificationsAsync(int userId, string userType, int count = 10)
    {
        try
        {
            var random = new Random();
            var notifications = new List<Notification>();

            var types = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>().ToList();
            var priorities = new[] { "Low", "Normal", "High" };
            var messages = new[]
            {
                "Yangi aksiya e'lon qilindi",
                "Sizning buyurtmangiz yetkazib berildi",
                "Sizning cashback balansigiz o'zgardi",
                "Yangi mahsulotlar qo'shildi",
                "Sizning so'rovingiz ko'rib chiqildi"
            };

            for (int i = 0; i < count; i++)
            {
                var notification = new Notification
                {
                    Title = $"Test xabarnoma #{i + 1}",
                    Message = messages[random.Next(messages.Length)],
                    NotificationType = types[random.Next(types.Count)],
                    Priority = priorities[random.Next(priorities.Length)],
                    UserId = userId,
                    UserType = Enum.Parse<UserType>(userType),
                    ActionUrl = "/test",
                    ActionText = "Ko'rish",
                    IsRead = random.Next(2) == 0
                };

                await _unitOfWork.Notifications.InsertAsync(notification);
                notifications.Add(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            var dtos = _mapper.Map<List<NotificationDto>>(notifications);
            return Result<List<NotificationDto>>.Success(dtos, $"{count} mock notifications created");
        }
        catch (Exception ex)
        {
            return Result<List<NotificationDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    // Support Chat Notifications
    public async Task NotifyAdminsAboutNewSupportChatAsync(int chatId)
    {
        try
        {
            var admins = await _unitOfWork.Admins
                .SelectAll(a => a.IsActive && a.DeletedAt == null)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    Title = "Yangi support chat",
                    Message = "Yangi mijoz support chatni boshladi",
                    NotificationType = NotificationType.SupportMessage,
                    Priority = "High",
                    UserId = admin.Id,
                    UserType = UserType.Admin,
                    ActionUrl = $"/admin/support/{chatId}",
                    ActionText = "Chatni ko'rish",
                    Data = chatId.ToString()
                };

                await _unitOfWork.Notifications.InsertAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error
        }
    }

    public async Task NotifyCustomerChatClosedAsync(int customerId, int chatId)
    {
        try
        {
            var notification = new Notification
            {
                Title = "Chat yopildi",
                Message = "Sizning support chatgiz yopildi",
                NotificationType = NotificationType.SupportMessage,
                Priority = "Normal",
                UserId = customerId,
                UserType = UserType.Customer,
                ActionUrl = $"/support/{chatId}",
                ActionText = "Chatni ko'rish",
                Data = chatId.ToString()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error
        }
    }

    public async Task NotifyAdminChatAssignedAsync(int adminId, int chatId)
    {
        try
        {
            var notification = new Notification
            {
                Title = "Yangi chat tayinlandi",
                Message = "Sizga yangi support chat tayinlandi",
                NotificationType = NotificationType.SupportMessage,
                Priority = "High",
                UserId = adminId,
                UserType = UserType.Admin,
                ActionUrl = $"/admin/support/{chatId}",
                ActionText = "Chatni ko'rish",
                Data = chatId.ToString()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error
        }
    }

    public async Task NotifyAdminNewMessageAsync(int adminId, int chatId)
    {
        try
        {
            var notification = new Notification
            {
                Title = "Yangi xabar",
                Message = "Sizga yangi xabar keldi",
                NotificationType = NotificationType.SupportMessage,
                Priority = "Normal",
                UserId = adminId,
                UserType = UserType.Admin,
                ActionUrl = $"/admin/support/{chatId}",
                ActionText = "Xabarni ko'rish",
                Data = chatId.ToString()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error
        }
    }

    public async Task NotifyCustomerNewMessageAsync(int customerId, int chatId)
    {
        try
        {
            var notification = new Notification
            {
                Title = "Yangi javob",
                Message = "Support jamoasidan yangi javob keldi",
                NotificationType = NotificationType.SupportMessage,
                Priority = "Normal",
                UserId = customerId,
                UserType = UserType.Customer,
                ActionUrl = $"/support/{chatId}",
                ActionText = "Javobni ko'rish",
                Data = chatId.ToString()
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error
        }
    }

    // ===== SODDA METODLAR =====

    /// <summary>
    /// Bitta foydalanuvchiga sodda notification yuborish
    /// </summary>
    public async Task<Result> SendSimpleNotificationAsync(SimpleNotificationDto request)
    {
        try
        {
            // Foydalanuvchini topish (Customer, Seller yoki Admin)
            var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == request.UserId && c.DeletedAt == null);
            UserType userType;

            if (customer != null)
            {
                userType = UserType.Customer;
            }
            else
            {
                var seller = await _unitOfWork.Sellers.FirstOrDefaultAsync(s => s.Id == request.UserId && s.DeletedAt == null);
                if (seller != null)
                {
                    userType = UserType.Seller;
                }
                else
                {
                    var admin = await _unitOfWork.Admins.FirstOrDefaultAsync(a => a.Id == request.UserId && a.DeletedAt == null);
                    if (admin != null)
                    {
                        userType = UserType.Admin;
                    }
                    else
                    {
                        return Result.NotFound("Foydalanuvchi topilmadi");
                    }
                }
            }

            // Notification yaratish
            var notification = new Notification
            {
                Title = request.Title,
                Message = request.Message,
                NotificationType = NotificationType.SystemMessage,
                Priority = "Normal",
                UserId = request.UserId,
                UserType = userType,
                Data = request.Data
            };

            await _unitOfWork.Notifications.InsertAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Firebase push notification yuborish (barcha qurilmalarga)
            var deviceTokens = await _unitOfWork.DeviceTokens.FindAsync(dt =>
                dt.UserId == request.UserId &&
                dt.UserType == userType &&
                dt.IsActive &&
                dt.DeletedAt == null);

            foreach (var deviceToken in deviceTokens)
            {
                try
                {
                    var data = new Dictionary<string, string>
                    {
                        { "notification_id", notification.Id.ToString() }
                    };

                    if (!string.IsNullOrEmpty(request.Data))
                    {
                        data.Add("custom_data", request.Data);
                    }

                    await _firebaseService.SendNotificationToUserAsync(
                        deviceToken.Token,
                        request.Title,
                        request.Message,
                        data
                    );
                }
                catch
                {
                    // Firebase yuborishda xatolik bo'lsa, davom etamiz
                }
            }

            return Result.Success("Notification muvaffaqiyatli yuborildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    /// <summary>
    /// Barcha foydalanuvchilarga ommaviy notification yuborish (UserType bo'yicha)
    /// </summary>
    public async Task<Result> SendBroadcastNotificationAsync(BroadcastNotificationDto request)
    {
        try
        {
            // UserType'ni parse qilish
            if (!Enum.TryParse<UserType>(request.UserType, out var userType))
            {
                return Result.BadRequest("Noto'g'ri UserType. Customer, Seller yoki Admin bo'lishi kerak");
            }

            // Barcha foydalanuvchilarni olish
            List<int> userIds = new List<int>();

            switch (userType)
            {
                case UserType.Customer:
                    var customers = await _unitOfWork.Customers.FindAsync(c => c.DeletedAt == null);
                    userIds = customers.Select(c => c.Id).ToList();
                    break;
                case UserType.Seller:
                    var sellers = await _unitOfWork.Sellers.FindAsync(s => s.DeletedAt == null);
                    userIds = sellers.Select(s => s.Id).ToList();
                    break;
                case UserType.Admin:
                    var admins = await _unitOfWork.Admins.FindAsync(a => a.DeletedAt == null);
                    userIds = admins.Select(a => a.Id).ToList();
                    break;
            }

            if (!userIds.Any())
            {
                return Result.NotFound($"{request.UserType} foydalanuvchilar topilmadi");
            }

            // Barcha foydalanuvchilarga notification yaratish
            var notifications = userIds.Select(userId => new Notification
            {
                Title = request.Title,
                Message = request.Message,
                NotificationType = NotificationType.Promotion,
                Priority = "High",
                UserId = userId,
                UserType = userType,
                Data = request.Data
            }).ToList();

            await _unitOfWork.Notifications.AddRangeAsync(notifications);
            await _unitOfWork.SaveChangesAsync();

            // Firebase push notification yuborish (barcha qurilmalarga)
            var deviceTokens = await _unitOfWork.DeviceTokens.FindAsync(dt =>
                dt.UserType == userType &&
                dt.IsActive &&
                dt.DeletedAt == null);

            int successCount = 0;
            foreach (var deviceToken in deviceTokens)
            {
                try
                {
                    var data = new Dictionary<string, string>
                    {
                        { "type", "broadcast" },
                        { "user_type", request.UserType }
                    };

                    if (!string.IsNullOrEmpty(request.Data))
                    {
                        data.Add("custom_data", request.Data);
                    }

                    await _firebaseService.SendNotificationToUserAsync(
                        deviceToken.Token,
                        request.Title,
                        request.Message,
                        data
                    );
                    successCount++;
                }
                catch
                {
                    // Firebase yuborishda xatolik bo'lsa, davom etamiz
                }
            }

            return Result.Success($"{notifications.Count} ta foydalanuvchiga notification yuborildi. {successCount} ta push notification yuborildi.");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }
}
