using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Notifications;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class DeviceTokenService : IDeviceTokenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFirebaseService _firebaseService;
    private readonly ILogger<DeviceTokenService> _logger;

    public DeviceTokenService(
        IUnitOfWork unitOfWork,
        IFirebaseService firebaseService,
        ILogger<DeviceTokenService> logger)
    {
        _unitOfWork = unitOfWork;
        _firebaseService = firebaseService;
        _logger = logger;
    }

    public async Task<Result> RegisterDeviceTokenAsync(int userId, UserType userType, RegisterDeviceTokenDto dto)
    {
        try
        {
            // Check if token already exists for this user
            var existingToken = await _unitOfWork.DeviceTokens.Table
                .FirstOrDefaultAsync(dt => dt.UserId == userId &&
                                          dt.UserType == userType &&
                                          dt.Token == dto.Token &&
                                          dt.DeletedAt == null);

            if (existingToken != null)
            {
                // Update existing token
                existingToken.DeviceName = dto.DeviceName;
                existingToken.DeviceOs = dto.DeviceOs;
                existingToken.AppVersion = dto.AppVersion;
                existingToken.IsActive = true;
                existingToken.LastUsedAt = DateTime.UtcNow;
                existingToken.MarkAsUpdated();

                await _unitOfWork.DeviceTokens.Update(existingToken, existingToken.Id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated device token for user {UserId}", userId);
                return Result.Success("Device token updated successfully");
            }

            // Create new token
            var deviceToken = new DeviceToken
            {
                UserId = userId,
                UserType = userType,
                Token = dto.Token,
                DeviceName = dto.DeviceName,
                DeviceOs = dto.DeviceOs,
                AppVersion = dto.AppVersion,
                IsActive = true,
                LastUsedAt = DateTime.UtcNow
            };

            await _unitOfWork.DeviceTokens.InsertAsync(deviceToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Registered new device token for user {UserId}", userId);
            return Result.Success("Device token registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token for user {UserId}", userId);
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<DeviceTokenResultDto>>> GetUserDeviceTokensAsync(int userId, UserType userType)
    {
        try
        {
            var tokens = await _unitOfWork.DeviceTokens.Table
                .Where(dt => dt.UserId == userId &&
                            dt.UserType == userType &&
                            dt.IsActive &&
                            dt.DeletedAt == null)
                .OrderByDescending(dt => dt.LastUsedAt)
                .Select(dt => new DeviceTokenResultDto
                {
                    Id = dt.Id,
                    UserId = dt.UserId,
                    UserType = dt.UserType.ToString(),
                    DeviceName = dt.DeviceName,
                    DeviceOs = dt.DeviceOs,
                    AppVersion = dt.AppVersion,
                    IsActive = dt.IsActive,
                    LastUsedAt = dt.LastUsedAt,
                    CreatedAt = dt.CreatedAt
                })
                .ToListAsync();

            return Result<List<DeviceTokenResultDto>>.Success(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device tokens for user {UserId}", userId);
            return Result<List<DeviceTokenResultDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeactivateDeviceTokenAsync(int userId, string token)
    {
        try
        {
            var deviceToken = await _unitOfWork.DeviceTokens.Table
                .FirstOrDefaultAsync(dt => dt.UserId == userId &&
                                          dt.Token == token &&
                                          dt.DeletedAt == null);

            if (deviceToken == null)
                return Result.NotFound("Device token not found");

            deviceToken.IsActive = false;
            deviceToken.MarkAsUpdated();

            await _unitOfWork.DeviceTokens.Update(deviceToken, deviceToken.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deactivated device token for user {UserId}", userId);
            return Result.Success("Device token deactivated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating device token for user {UserId}", userId);
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeactivateAllUserTokensAsync(int userId, UserType userType)
    {
        try
        {
            var tokens = await _unitOfWork.DeviceTokens.Table
                .Where(dt => dt.UserId == userId &&
                            dt.UserType == userType &&
                            dt.IsActive &&
                            dt.DeletedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsActive = false;
                token.MarkAsUpdated();
                await _unitOfWork.DeviceTokens.Update(token, token.Id);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deactivated all device tokens for user {UserId}", userId);
            return Result.Success($"{tokens.Count} device tokens deactivated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating all tokens for user {UserId}", userId);
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> CleanupExpiredTokensAsync()
    {
        try
        {
            var expiredDate = DateTime.UtcNow.AddDays(-60);

            var expiredTokens = await _unitOfWork.DeviceTokens.Table
                .Where(dt => dt.LastUsedAt.HasValue &&
                            dt.LastUsedAt.Value < expiredDate &&
                            dt.DeletedAt == null)
                .ToListAsync();

            foreach (var token in expiredTokens)
            {
                token.Delete();
                await _unitOfWork.DeviceTokens.Update(token, token.Id);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired device tokens", expiredTokens.Count);
            return Result.Success($"{expiredTokens.Count} expired tokens cleaned up");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired tokens");
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> SendPushNotificationAsync(SendPushNotificationDto dto)
    {
        try
        {
            // Get all active device tokens for the user
            var tokens = await _unitOfWork.DeviceTokens.Table
                .Where(dt => dt.UserId == dto.UserId &&
                            dt.IsActive &&
                            dt.DeletedAt == null)
                .Select(dt => dt.Token)
                .ToListAsync();

            if (tokens.Count == 0)
            {
                _logger.LogWarning("No active device tokens found for user {UserId}", dto.UserId);
                return Result.BadRequest("No active devices found for this user");
            }

            // Send to all user's devices
            int successCount = await _firebaseService.SendNotificationToMultipleUsersAsync(
                tokens,
                dto.Title,
                dto.Message,
                dto.Data
            );

            _logger.LogInformation("Sent notification to {SuccessCount}/{TotalCount} devices for user {UserId}",
                successCount, tokens.Count, dto.UserId);

            return Result.Success($"Notification sent to {successCount}/{tokens.Count} devices");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", dto.UserId);
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> SendBatchPushNotificationAsync(SendBatchPushNotificationDto dto)
    {
        try
        {
            // Get all active device tokens for all specified users
            var tokens = await _unitOfWork.DeviceTokens.Table
                .Where(dt => dto.UserIds.Contains(dt.UserId) &&
                            dt.IsActive &&
                            dt.DeletedAt == null)
                .Select(dt => dt.Token)
                .ToListAsync();

            if (tokens.Count == 0)
            {
                _logger.LogWarning("No active device tokens found for specified users");
                return Result.BadRequest("No active devices found for specified users");
            }

            // Send to all devices
            int successCount = await _firebaseService.SendNotificationToMultipleUsersAsync(
                tokens,
                dto.Title,
                dto.Message,
                dto.Data
            );

            _logger.LogInformation("Batch notification sent to {SuccessCount}/{TotalCount} devices",
                successCount, tokens.Count);

            return Result.Success($"Notification sent to {successCount}/{tokens.Count} devices across {dto.UserIds.Count} users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch push notification");
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> SendTopicNotificationAsync(SendTopicNotificationDto dto)
    {
        try
        {
            bool success = await _firebaseService.SendNotificationToTopicAsync(
                dto.Topic,
                dto.Title,
                dto.Message,
                dto.Data
            );

            if (success)
            {
                _logger.LogInformation("Topic notification sent to topic {Topic}", dto.Topic);
                return Result.Success($"Notification sent to topic '{dto.Topic}'");
            }
            else
            {
                _logger.LogWarning("Failed to send topic notification to {Topic}", dto.Topic);
                return Result.BadRequest("Failed to send topic notification");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending topic notification to {Topic}", dto.Topic);
            return Result.InternalError($"Error: {ex.Message}");
        }
    }
}
