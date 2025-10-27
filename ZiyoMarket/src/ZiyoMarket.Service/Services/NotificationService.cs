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

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

            // TODO: Send push notification if needed

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
                    !n.IsDeleted)
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
                !n.IsDeleted);

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
                    !n.IsDeleted)
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
                .SelectAll(a => a.IsActive && !a.IsDeleted)
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
            var query = _unitOfWork.Notifications.Table.Where(n => !n.IsDeleted);

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
                .SelectAll(a => a.IsActive && !a.IsDeleted)
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
}
