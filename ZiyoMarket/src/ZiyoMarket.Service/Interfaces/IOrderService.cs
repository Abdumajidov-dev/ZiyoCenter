using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface IOrderService
{
    // CRUD
    Task<Result<OrderDetailDto>> GetOrderByIdAsync(int orderId, int userId, string userType);
    Task<Result<PaginationResponse<OrderListDto>>> GetOrdersAsync(OrderFilterRequest request, int userId, string userType);
    Task<Result<OrderDetailDto>> CreateOrderAsync(CreateOrderDto request, int customerId);
    Task<Result<OrderDetailDto>> CreateOrderBySellerAsync(CreateOrderDto request, int sellerId);
    Task<Result> CancelOrderAsync(int orderId, int userId, string userType, string? reason);

    // Status management
    Task<Result> UpdateOrderStatusAsync(int orderId, string status, int updatedBy);
    Task<Result> ConfirmOrderAsync(int orderId, int sellerId);
    Task<Result> MarkAsReadyForPickupAsync(int orderId, int updatedBy);
    Task<Result> MarkAsShippedAsync(int orderId, int updatedBy);
    Task<Result> MarkAsDeliveredAsync(int orderId, int updatedBy);

    // Discount
    Task<Result> ApplyDiscountAsync(ApplyDiscountDto request, int sellerId);
    Task<Result> RemoveDiscountAsync(int orderDiscountId, int sellerId);

    // Payment
    Task<Result> ProcessPaymentAsync(int orderId, string paymentMethod, string? reference);

    // Statistics
    Task<Result<OrderSummaryDto>> GetOrderSummaryAsync(DateTime dateFrom, DateTime dateTo);
    Task<Result<List<OrderListDto>>> GetCustomerOrdersAsync(int customerId);
    Task<Result<List<OrderListDto>>> GetSellerOrdersAsync(int sellerId);

    // Bulk operations
    Task<Result> DeleteAllOrdersAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<OrderDetailDto>>> SeedMockOrdersAsync(int createdBy, int count = 10, DateTime? startDate = null, DateTime? endDate = null);
}