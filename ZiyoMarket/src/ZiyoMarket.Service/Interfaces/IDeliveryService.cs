using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Delivery;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IDeliveryService
{
    // Delivery Partner
    Task<Result<DeliveryPartnerDto>> CreatePartnerAsync(DeliveryPartnerCreateDto deliveryPartnerCreateDto, int createdBy);
    Task<Result<DeliveryPartnerDto>> UpdatePartnerAsync(int id, DeliveryPartnerUpdateDto dto, int updatedBy);
    Task<Result<bool>> DeletePartnerAsync(int id);
    Task<Result<List<DeliveryPartnerDto>>> GetAllPartnersAsync(bool onlyActive = false);
    Task<Result<DeliveryPartnerDto>> GetPartnerByIdAsync(int id);
    Task<Result<DeliveryPartnerStatsDto>> GetPartnerStatsAsync(int id);
    Task<Result<bool>> SetPartnerActiveStatusAsync(int id, bool isActive);

    // Order Delivery
    Task<Result<OrderDeliveryDto>> AssignDeliveryAsync(OrderDeliveryCreateDto dto);
    Task<Result<OrderDeliveryDto>> UpdateDeliveryStatusAsync(int id, DeliveryStatus newStatus, string? reason = null);
    Task<Result<List<OrderDeliveryDto>>> GetDeliveriesByPartnerAsync(int partnerId);
    Task<Result<List<OrderDeliveryDto>>> GetDelayedDeliveriesAsync();
}
