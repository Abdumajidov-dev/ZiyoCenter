using ZiyoMarket.Service.DTOs.Delivery;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IDeliveryService
{
    // Delivery Partner Operations
    Task<Result<List<DeliveryPartnerDto>>> GetAllDeliveryPartnersAsync();
    Task<Result<List<DeliveryPartnerDto>>> GetActiveDeliveryPartnersAsync();
    Task<Result<DeliveryPartnerDto>> GetDeliveryPartnerByIdAsync(int id);
    Task<Result<DeliveryPartnerDto>> CreateDeliveryPartnerAsync(SaveDeliveryPartnerDto request, int createdBy);
    Task<Result<DeliveryPartnerDto>> UpdateDeliveryPartnerAsync(int id, SaveDeliveryPartnerDto request, int updatedBy);
    Task<Result> DeleteDeliveryPartnerAsync(int id, int deletedBy);
    Task<Result<DeliveryPartnerStatsDto>> GetPartnerStatsAsync(int id);
    Task<Result<bool>> SetPartnerActiveStatusAsync(int id, bool isActive);

    // Order Delivery Operations
    Task<Result<OrderDeliveryDto>> GetOrderDeliveryAsync(int orderId);
    Task<Result<OrderDeliveryDto>> CreateOrderDeliveryAsync(CreateOrderDeliveryDto request, int createdBy);
    Task<Result<OrderDeliveryDto>> UpdateDeliveryStatusAsync(int id, string status, int updatedBy);
    Task<Result<OrderDeliveryDto>> GetDeliveryByTrackingCodeAsync(string trackingCode);
    Task<Result<List<OrderDeliveryDto>>> GetDeliveriesByPartnerAsync(int partnerId);
    Task<Result<List<OrderDeliveryDto>>> GetDelayedDeliveriesAsync();

    // Bulk Operations
    Task<Result> DeleteAllDeliveryPartnersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<DeliveryPartnerDto>>> SeedMockDeliveryPartnersAsync(int createdBy, int count = 3);
}
