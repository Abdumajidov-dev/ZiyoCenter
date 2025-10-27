using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Delivery;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Delivery management service implementation
/// </summary>
public class DeliveryService : IDeliveryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeliveryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    #region Delivery Partner Operations

    public async Task<Result<List<DeliveryPartnerDto>>> GetAllDeliveryPartnersAsync()
    {
        try
        {
            var partners = await _unitOfWork.DeliveryPartners.Table
                .Include(dp => dp.OrderDeliveries)
                .Where(dp => !dp.IsDeleted)
                .OrderBy(dp => dp.DisplayOrder)
                .ThenBy(dp => dp.Name)
                .ToListAsync();

            var dtos = partners.Select(p => new DeliveryPartnerDto
            {
                Id = p.Id,
                Name = p.Name,
                DeliveryType = p.DeliveryType,
                PricePerDelivery = p.PricePerDelivery,
                EstimatedDays = p.EstimatedDays,
                IsActive = p.IsActive,
                TotalDeliveries = p.GetTotalDeliveries(),
                SuccessRate = p.GetSuccessRate()
            }).ToList();

            return Result<List<DeliveryPartnerDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<DeliveryPartnerDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<DeliveryPartnerDto>>> GetActiveDeliveryPartnersAsync()
    {
        try
        {
            var partners = await _unitOfWork.DeliveryPartners.Table
                .Include(dp => dp.OrderDeliveries)
                .Where(dp => !dp.IsDeleted && dp.IsActive)
                .OrderBy(dp => dp.DisplayOrder)
                .ThenBy(dp => dp.Name)
                .ToListAsync();

            var dtos = partners.Select(p => new DeliveryPartnerDto
            {
                Id = p.Id,
                Name = p.Name,
                DeliveryType = p.DeliveryType,
                PricePerDelivery = p.PricePerDelivery,
                EstimatedDays = p.EstimatedDays,
                IsActive = p.IsActive,
                TotalDeliveries = p.GetTotalDeliveries(),
                SuccessRate = p.GetSuccessRate()
            }).ToList();

            return Result<List<DeliveryPartnerDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<DeliveryPartnerDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<DeliveryPartnerDto>> GetDeliveryPartnerByIdAsync(int id)
    {
        try
        {
            var partner = await _unitOfWork.DeliveryPartners
                .SelectAsync(dp => dp.Id == id && !dp.IsDeleted, new[] { "OrderDeliveries" });

            if (partner == null)
                return Result<DeliveryPartnerDto>.NotFound("Yetkazib berish hamkori topilmadi");

            var dto = new DeliveryPartnerDto
            {
                Id = partner.Id,
                Name = partner.Name,
                DeliveryType = partner.DeliveryType,
                PricePerDelivery = partner.PricePerDelivery,
                EstimatedDays = partner.EstimatedDays,
                IsActive = partner.IsActive,
                TotalDeliveries = partner.GetTotalDeliveries(),
                SuccessRate = partner.GetSuccessRate()
            };

            return Result<DeliveryPartnerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DeliveryPartnerDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<DeliveryPartnerDto>> CreateDeliveryPartnerAsync(SaveDeliveryPartnerDto request, int createdBy)
    {
        try
        {
            // Check if partner with same name exists
            var existingPartner = await _unitOfWork.DeliveryPartners.Table
                .FirstOrDefaultAsync(dp => dp.Name == request.Name && !dp.IsDeleted);

            if (existingPartner != null)
                return Result<DeliveryPartnerDto>.BadRequest("Bu nom bilan yetkazib berish hamkori allaqachon mavjud");

            var partner = new DeliveryPartner
            {
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                DeliveryType = request.DeliveryType,
                PricePerDelivery = request.PricePerDelivery,
                EstimatedDays = request.EstimatedDays,
                MaxWeight = request.MaxWeight,
                ServiceAreas = request.ServiceAreas,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                ApiUrl = request.ApiUrl,
                ApiKey = request.ApiKey,
                Notes = request.Notes,
                CreatedBy = createdBy
            };

            partner.MarkAsCreated();

            // Validate
            var validationResult = partner.Validate();
            if (!validationResult.IsSuccess)
                return Result<DeliveryPartnerDto>.BadRequest(validationResult.Message);

            await _unitOfWork.DeliveryPartners.InsertAsync(partner);
            await _unitOfWork.SaveAsync();

            var dto = new DeliveryPartnerDto
            {
                Id = partner.Id,
                Name = partner.Name,
                DeliveryType = partner.DeliveryType,
                PricePerDelivery = partner.PricePerDelivery,
                EstimatedDays = partner.EstimatedDays,
                IsActive = partner.IsActive,
                TotalDeliveries = 0,
                SuccessRate = 100
            };

            return Result<DeliveryPartnerDto>.Success(dto, "Yetkazib berish hamkori muvaffaqiyatli yaratildi");
        }
        catch (Exception ex)
        {
            return Result<DeliveryPartnerDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<DeliveryPartnerDto>> UpdateDeliveryPartnerAsync(int id, SaveDeliveryPartnerDto request, int updatedBy)
    {
        try
        {
            var partner = await _unitOfWork.DeliveryPartners
                .SelectAsync(dp => dp.Id == id && !dp.IsDeleted, new[] { "OrderDeliveries" });

            if (partner == null)
                return Result<DeliveryPartnerDto>.NotFound("Yetkazib berish hamkori topilmadi");

            // Check if name is unique (excluding current partner)
            var existingPartner = await _unitOfWork.DeliveryPartners.Table
                .FirstOrDefaultAsync(dp => dp.Name == request.Name && dp.Id != id && !dp.IsDeleted);

            if (existingPartner != null)
                return Result<DeliveryPartnerDto>.BadRequest("Bu nom bilan yetkazib berish hamkori allaqachon mavjud");

            // Update properties
            partner.ChangeName(request.Name);
            partner.UpdatePhone(request.Phone);
            partner.UpdateEmail(request.Email);
            partner.DeliveryType = request.DeliveryType;
            partner.UpdatePrice(request.PricePerDelivery);
            partner.UpdateEstimatedDays(request.EstimatedDays);
            partner.SetMaxWeight(request.MaxWeight);
            partner.UpdateServiceAreas(request.ServiceAreas);
            partner.IsActive = request.IsActive;
            partner.DisplayOrder = request.DisplayOrder;
            partner.UpdateApiInfo(request.ApiUrl, request.ApiKey);
            partner.Notes = request.Notes;
            partner.UpdatedBy = updatedBy;
            partner.MarkAsUpdated();

            // Validate
            var validationResult = partner.Validate();
            if (!validationResult.IsSuccess)
                return Result<DeliveryPartnerDto>.BadRequest(validationResult.Message);

            _unitOfWork.DeliveryPartners.Update(partner);
            await _unitOfWork.SaveAsync();

            var dto = new DeliveryPartnerDto
            {
                Id = partner.Id,
                Name = partner.Name,
                DeliveryType = partner.DeliveryType,
                PricePerDelivery = partner.PricePerDelivery,
                EstimatedDays = partner.EstimatedDays,
                IsActive = partner.IsActive,
                TotalDeliveries = partner.GetTotalDeliveries(),
                SuccessRate = partner.GetSuccessRate()
            };

            return Result<DeliveryPartnerDto>.Success(dto, "Yetkazib berish hamkori muvaffaqiyatli yangilandi");
        }
        catch (Exception ex)
        {
            return Result<DeliveryPartnerDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result> DeleteDeliveryPartnerAsync(int id, int deletedBy)
    {
        try
        {
            var partner = await _unitOfWork.DeliveryPartners
                .SelectAsync(dp => dp.Id == id && !dp.IsDeleted, new[] { "OrderDeliveries" });

            if (partner == null)
                return Result.NotFound("Yetkazib berish hamkori topilmadi");

            // Check if partner has active deliveries
            var hasActiveDeliveries = partner.OrderDeliveries
                .Any(od => !od.IsDeleted && od.DeliveryStatus != DeliveryStatus.Delivered && od.DeliveryStatus != DeliveryStatus.Failed);

            if (hasActiveDeliveries)
                return Result.BadRequest("Bu hamkor orqali faol yetkazib berishlar mavjud. Avval ularni yakunlang");

            partner.DeletedBy = deletedBy;
            partner.MarkAsDeleted();
            _unitOfWork.DeliveryPartners.Delete(partner);
            await _unitOfWork.SaveAsync();

            return Result.Success("Yetkazib berish hamkori muvaffaqiyatli o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<DeliveryPartnerStatsDto>> GetPartnerStatsAsync(int id)
    {
        try
        {
            var partner = await _unitOfWork.DeliveryPartners
                .SelectAsync(dp => dp.Id == id && !dp.IsDeleted, new[] { "OrderDeliveries" });

            if (partner == null)
                return Result<DeliveryPartnerStatsDto>.NotFound("Yetkazib berish hamkori topilmadi");

            var stats = new DeliveryPartnerStatsDto
            {
                PartnerId = partner.Id,
                PartnerName = partner.Name,
                TotalDeliveries = partner.OrderDeliveries.Count,
                SuccessfulDeliveries = partner.OrderDeliveries.Count(od => od.DeliveryStatus == DeliveryStatus.Delivered),
                FailedDeliveries = partner.OrderDeliveries.Count(od => od.DeliveryStatus == DeliveryStatus.Failed),
                InProgressDeliveries = partner.OrderDeliveries.Count(od => od.IsInProgress),
                SuccessRate = partner.GetSuccessRate(),
                AverageDeliveryDays = partner.GetAverageDeliveryDays()
            };

            return Result<DeliveryPartnerStatsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            return Result<DeliveryPartnerStatsDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SetPartnerActiveStatusAsync(int id, bool isActive)
    {
        try
        {
            var partner = await _unitOfWork.DeliveryPartners
                .SelectAsync(dp => dp.Id == id && !dp.IsDeleted);

            if (partner == null)
                return Result<bool>.NotFound("Yetkazib berish hamkori topilmadi");

            if (isActive)
                partner.Activate();
            else
                partner.Deactivate();

            _unitOfWork.DeliveryPartners.Update(partner);
            await _unitOfWork.SaveAsync();

            return Result<bool>.Success(true, $"Hamkor {(isActive ? "faollashtirildi" : "faolsizlashtirildi")}");
        }
        catch (Exception ex)
        {
            return Result<bool>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Order Delivery Operations

    public async Task<Result<OrderDeliveryDto>> GetOrderDeliveryAsync(int orderId)
    {
        try
        {
            var orderDelivery = await _unitOfWork.OrderDeliveries.Table
                .Include(od => od.Order)
                .Include(od => od.DeliveryPartner)
                .FirstOrDefaultAsync(od => od.OrderId == orderId && !od.IsDeleted);

            if (orderDelivery == null)
                return Result<OrderDeliveryDto>.NotFound("Buyurtma yetkazib berish ma'lumotlari topilmadi");

            var dto = new OrderDeliveryDto
            {
                Id = orderDelivery.Id,
                OrderId = orderDelivery.OrderId,
                OrderNumber = orderDelivery.Order?.OrderNumber ?? "",
                DeliveryPartnerId = orderDelivery.DeliveryPartnerId,
                DeliveryPartnerName = orderDelivery.DeliveryPartner?.Name ?? "",
                DeliveryStatus = orderDelivery.DeliveryStatus,
                DeliveryFee = orderDelivery.DeliveryFee,
                AssignedAt = orderDelivery.AssignedAt,
                DeliveredAt = orderDelivery.DeliveredAt,
                IsDelayed = orderDelivery.IsDelayed
            };

            return Result<OrderDeliveryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrderDeliveryDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<OrderDeliveryDto>> CreateOrderDeliveryAsync(CreateOrderDeliveryDto request, int createdBy)
    {
        try
        {
            // Check if order exists
            var order = await _unitOfWork.Orders
                .SelectAsync(o => o.Id == request.OrderId && !o.IsDeleted);

            if (order == null)
                return Result<OrderDeliveryDto>.NotFound("Buyurtma topilmadi");

            // Check if order already has delivery
            var existingDelivery = await _unitOfWork.OrderDeliveries.Table
                .FirstOrDefaultAsync(od => od.OrderId == request.OrderId && !od.IsDeleted);

            if (existingDelivery != null)
                return Result<OrderDeliveryDto>.BadRequest("Bu buyurtma uchun yetkazib berish allaqachon yaratilgan");

            // Check if delivery partner exists and is active
            var partner = await _unitOfWork.DeliveryPartners
                .SelectAsync(dp => dp.Id == request.DeliveryPartnerId && !dp.IsDeleted);

            if (partner == null)
                return Result<OrderDeliveryDto>.NotFound("Yetkazib berish hamkori topilmadi");

            if (!partner.IsActive)
                return Result<OrderDeliveryDto>.BadRequest("Bu yetkazib berish hamkori faol emas");

            // Create order delivery
            var orderDelivery = new OrderDelivery
            {
                OrderId = request.OrderId,
                DeliveryPartnerId = request.DeliveryPartnerId,
                DeliveryAddress = request.DeliveryAddress,
                DeliveryFee = request.DeliveryFee,
                ReceiverName = request.ReceiverName,
                ReceiverPhone = request.ReceiverPhone,
                Notes = request.Notes,
                EstimatedDeliveryDate = request.EstimatedDeliveryDate ?? DateTime.UtcNow.AddDays(partner.EstimatedDays),
                TrackingCode = OrderDelivery.GenerateTrackingCode(),
                DeliveryStatus = DeliveryStatus.Assigned,
                AssignedAt = DateTime.UtcNow
            };

            // Validate
            var validationResult = orderDelivery.Validate();
            if (!validationResult.IsSuccess)
                return Result<OrderDeliveryDto>.BadRequest(validationResult.Message);

            await _unitOfWork.OrderDeliveries.InsertAsync(orderDelivery);
            await _unitOfWork.SaveAsync();

            var dto = new OrderDeliveryDto
            {
                Id = orderDelivery.Id,
                OrderId = orderDelivery.OrderId,
                OrderNumber = order.OrderNumber,
                DeliveryPartnerId = orderDelivery.DeliveryPartnerId,
                DeliveryPartnerName = partner.Name,
                DeliveryStatus = orderDelivery.DeliveryStatus,
                DeliveryFee = orderDelivery.DeliveryFee,
                AssignedAt = orderDelivery.AssignedAt,
                DeliveredAt = orderDelivery.DeliveredAt,
                IsDelayed = orderDelivery.IsDelayed
            };

            return Result<OrderDeliveryDto>.Success(dto, "Yetkazib berish muvaffaqiyatli yaratildi");
        }
        catch (Exception ex)
        {
            return Result<OrderDeliveryDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<OrderDeliveryDto>> UpdateDeliveryStatusAsync(int id, string status, int updatedBy)
    {
        try
        {
            var orderDelivery = await _unitOfWork.OrderDeliveries
                .SelectAsync(od => od.Id == id && !od.IsDeleted,
                    new[] { "Order", "DeliveryPartner" });

            if (orderDelivery == null)
                return Result<OrderDeliveryDto>.NotFound("Yetkazib berish topilmadi");

            // Parse status
            if (!Enum.TryParse<DeliveryStatus>(status, true, out var newStatus))
                return Result<OrderDeliveryDto>.BadRequest("Noto'g'ri status");

            // Update status based on new status
            switch (newStatus)
            {
                case DeliveryStatus.PickedUp:
                    orderDelivery.PickUp();
                    break;
                case DeliveryStatus.InTransit:
                    orderDelivery.MarkInTransit();
                    break;
                case DeliveryStatus.Delivered:
                    orderDelivery.Deliver();
                    break;
                case DeliveryStatus.Failed:
                    orderDelivery.MarkAsFailed("Status changed by admin");
                    break;
                default:
                    orderDelivery.DeliveryStatus = newStatus;
                    break;
            }

            orderDelivery.MarkAsUpdated();
            _unitOfWork.OrderDeliveries.Update(orderDelivery);
            await _unitOfWork.SaveAsync();

            var dto = new OrderDeliveryDto
            {
                Id = orderDelivery.Id,
                OrderId = orderDelivery.OrderId,
                OrderNumber = orderDelivery.Order?.OrderNumber ?? "",
                DeliveryPartnerId = orderDelivery.DeliveryPartnerId,
                DeliveryPartnerName = orderDelivery.DeliveryPartner?.Name ?? "",
                DeliveryStatus = orderDelivery.DeliveryStatus,
                DeliveryFee = orderDelivery.DeliveryFee,
                AssignedAt = orderDelivery.AssignedAt,
                DeliveredAt = orderDelivery.DeliveredAt,
                IsDelayed = orderDelivery.IsDelayed
            };

            return Result<OrderDeliveryDto>.Success(dto, "Yetkazib berish statusi yangilandi");
        }
        catch (InvalidOperationException ex)
        {
            return Result<OrderDeliveryDto>.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<OrderDeliveryDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<OrderDeliveryDto>> GetDeliveryByTrackingCodeAsync(string trackingCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
                return Result<OrderDeliveryDto>.BadRequest("Kuzatuv kodi bo'sh bo'lmasligi kerak");

            var orderDelivery = await _unitOfWork.OrderDeliveries.Table
                .Include(od => od.Order)
                .Include(od => od.DeliveryPartner)
                .FirstOrDefaultAsync(od => od.TrackingCode == trackingCode && !od.IsDeleted);

            if (orderDelivery == null)
                return Result<OrderDeliveryDto>.NotFound("Yetkazib berish topilmadi");

            var dto = new OrderDeliveryDto
            {
                Id = orderDelivery.Id,
                OrderId = orderDelivery.OrderId,
                OrderNumber = orderDelivery.Order?.OrderNumber ?? "",
                DeliveryPartnerId = orderDelivery.DeliveryPartnerId,
                DeliveryPartnerName = orderDelivery.DeliveryPartner?.Name ?? "",
                DeliveryStatus = orderDelivery.DeliveryStatus,
                DeliveryFee = orderDelivery.DeliveryFee,
                AssignedAt = orderDelivery.AssignedAt,
                DeliveredAt = orderDelivery.DeliveredAt,
                IsDelayed = orderDelivery.IsDelayed
            };

            return Result<OrderDeliveryDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<OrderDeliveryDto>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<OrderDeliveryDto>>> GetDeliveriesByPartnerAsync(int partnerId)
    {
        try
        {
            var deliveries = await _unitOfWork.OrderDeliveries.Table
                .Include(od => od.Order)
                .Include(od => od.DeliveryPartner)
                .Where(od => od.DeliveryPartnerId == partnerId && !od.IsDeleted)
                .OrderByDescending(od => od.AssignedAt)
                .ToListAsync();

            var dtos = deliveries.Select(od => new OrderDeliveryDto
            {
                Id = od.Id,
                OrderId = od.OrderId,
                OrderNumber = od.Order?.OrderNumber ?? "",
                DeliveryPartnerId = od.DeliveryPartnerId,
                DeliveryPartnerName = od.DeliveryPartner?.Name ?? "",
                DeliveryStatus = od.DeliveryStatus,
                DeliveryFee = od.DeliveryFee,
                AssignedAt = od.AssignedAt,
                DeliveredAt = od.DeliveredAt,
                IsDelayed = od.IsDelayed
            }).ToList();

            return Result<List<OrderDeliveryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<OrderDeliveryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<OrderDeliveryDto>>> GetDelayedDeliveriesAsync()
    {
        try
        {
            var deliveries = await _unitOfWork.OrderDeliveries.Table
                .Include(od => od.Order)
                .Include(od => od.DeliveryPartner)
                .Where(od => !od.IsDeleted &&
                            od.EstimatedDeliveryDate.HasValue &&
                            od.EstimatedDeliveryDate.Value < DateTime.UtcNow &&
                            od.DeliveryStatus != DeliveryStatus.Delivered &&
                            od.DeliveryStatus != DeliveryStatus.Failed)
                .OrderBy(od => od.EstimatedDeliveryDate)
                .ToListAsync();

            var dtos = deliveries.Select(od => new OrderDeliveryDto
            {
                Id = od.Id,
                OrderId = od.OrderId,
                OrderNumber = od.Order?.OrderNumber ?? "",
                DeliveryPartnerId = od.DeliveryPartnerId,
                DeliveryPartnerName = od.DeliveryPartner?.Name ?? "",
                DeliveryStatus = od.DeliveryStatus,
                DeliveryFee = od.DeliveryFee,
                AssignedAt = od.AssignedAt,
                DeliveredAt = od.DeliveredAt,
                IsDelayed = od.IsDelayed
            }).ToList();

            return Result<List<OrderDeliveryDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<OrderDeliveryDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion

    #region Bulk Operations

    public async Task<Result> DeleteAllDeliveryPartnersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _unitOfWork.DeliveryPartners.Table
                .Include(dp => dp.OrderDeliveries)
                .Where(dp => !dp.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(dp => dp.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(dp => dp.CreatedAt <= endDate.Value);

            var partners = await query.ToListAsync();

            // Only delete partners without active deliveries
            var deletablePartners = partners
                .Where(p => !p.OrderDeliveries.Any(od => !od.IsDeleted &&
                    od.DeliveryStatus != DeliveryStatus.Delivered &&
                    od.DeliveryStatus != DeliveryStatus.Failed))
                .ToList();

            if (!deletablePartners.Any())
                return Result.Success("O'chirilishi mumkin bo'lgan yetkazib berish hamkorlari topilmadi");

            foreach (var partner in deletablePartners)
            {
                partner.DeletedBy = deletedBy;
                partner.MarkAsDeleted();
                _unitOfWork.DeliveryPartners.Delete(partner);
            }

            await _unitOfWork.SaveAsync();

            return Result.Success($"{deletablePartners.Count} ta yetkazib berish hamkori o'chirildi");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Xatolik: {ex.Message}");
        }
    }

    public async Task<Result<List<DeliveryPartnerDto>>> SeedMockDeliveryPartnersAsync(int createdBy, int count = 3)
    {
        try
        {
            var mockPartners = new List<DeliveryPartner>();

            var partnerData = new[]
            {
                new { Name = "Yandex Delivery", Type = "Express", Price = 15000m, Days = 1 },
                new { Name = "O'zbekiston Pochta", Type = "Postal", Price = 8000m, Days = 3 },
                new { Name = "Uzum Tezkor", Type = "Courier", Price = 12000m, Days = 1 },
                new { Name = "DHL Express", Type = "Express", Price = 50000m, Days = 1 },
                new { Name = "Mahalliy Yetkazish", Type = "Courier", Price = 10000m, Days = 2 }
            };

            for (int i = 0; i < Math.Min(count, partnerData.Length); i++)
            {
                var data = partnerData[i];
                var partner = new DeliveryPartner
                {
                    Name = data.Name,
                    DeliveryType = data.Type,
                    PricePerDelivery = data.Price,
                    EstimatedDays = data.Days,
                    Phone = $"+998901234{(i + 1):D3}",
                    Email = $"{data.Name.ToLower().Replace(" ", "")}@example.com",
                    IsActive = true,
                    DisplayOrder = i + 1,
                    ServiceAreas = "Toshkent, Samarqand, Buxoro",
                    Notes = $"Mock test data for {data.Name}",
                    CreatedBy = createdBy
                };

                partner.MarkAsCreated();
                await _unitOfWork.DeliveryPartners.InsertAsync(partner);
                mockPartners.Add(partner);
            }

            await _unitOfWork.SaveAsync();

            var dtos = mockPartners.Select(p => new DeliveryPartnerDto
            {
                Id = p.Id,
                Name = p.Name,
                DeliveryType = p.DeliveryType,
                PricePerDelivery = p.PricePerDelivery,
                EstimatedDays = p.EstimatedDays,
                IsActive = p.IsActive,
                TotalDeliveries = 0,
                SuccessRate = 100
            }).ToList();

            return Result<List<DeliveryPartnerDto>>.Success(dtos, $"{mockPartners.Count} ta mock yetkazib berish hamkori yaratildi");
        }
        catch (Exception ex)
        {
            return Result<List<DeliveryPartnerDto>>.InternalError($"Xatolik: {ex.Message}");
        }
    }

    #endregion
}
