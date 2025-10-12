using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.DTOs.Cashback;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

/// <summary>
/// Order management service interface
/// </summary>
public interface IOrderService
{
    // ============ Order CRUD ============
    
    /// <summary>
    /// Get order by ID
    /// </summary>
    Task<Result<OrderDetailDto>> GetOrderByIdAsync(int orderId, int userId, string userType);
    
    /// <summary>
    /// Get orders with pagination and filtering
    /// </summary>
    Task<Result<PaginationResponse<OrderListDto>>> GetOrdersAsync(
        OrderFilterRequest request, int userId, string userType);
    
    /// <summary>
    /// Create order (by customer)
    /// </summary>
    Task<Result<OrderDetailDto>> CreateOrderAsync(CreateOrderDto request, int customerId);
    
    /// <summary>
    /// Create order by seller (in-store)
    /// </summary>
    Task<Result<OrderDetailDto>> CreateOrderBySellerAsync(
        CreateOrderDto request, int sellerId);
    
    /// <summary>
    /// Cancel order
    /// </summary>
    Task<Result> CancelOrderAsync(int orderId, int userId, string userType, string? reason);
    
    // ============ Order Status Management ============
    
    /// <summary>
    /// Update order status
    /// </summary>
    Task<Result> UpdateOrderStatusAsync(UpdateOrderStatusDto request, int updatedBy);
    
    /// <summary>
    /// Confirm order (by seller or admin)
    /// </summary>
    Task<Result> ConfirmOrderAsync(int orderId, int sellerId);
    
    /// <summary>
    /// Mark order as ready for pickup
    /// </summary>
    Task<Result> MarkAsReadyForPickupAsync(int orderId, int updatedBy);
    
    /// <summary>
    /// Mark order as shipped
    /// </summary>
    Task<Result> MarkAsShippedAsync(int orderId, int updatedBy);
    
    /// <summary>
    /// Mark order as delivered
    /// </summary>
    Task<Result> MarkAsDeliveredAsync(int orderId, int updatedBy);
    
    // ============ Discount Management ============
    
    /// <summary>
    /// Apply discount to order
    /// </summary>
    Task<Result> ApplyDiscountAsync(ApplyDiscountDto request, int sellerId);
    
    /// <summary>
    /// Remove discount from order
    /// </summary>
    Task<Result> RemoveDiscountAsync(int orderDiscountId, int sellerId);
    
    /// <summary>
    /// Get discount reasons
    /// </summary>
    Task<Result<List<DiscountReasonDto>>> GetDiscountReasonsAsync();
    
    // ============ Payment ============
    
    /// <summary>
    /// Process payment for order
    /// </summary>
    Task<Result> ProcessPaymentAsync(int orderId, string paymentMethod, string? reference);
    
    /// <summary>
    /// Refund order
    /// </summary>
    Task<Result> RefundOrderAsync(int orderId, int adminId, string reason);
    
    // ============ Order Summary ============
    
    /// <summary>
    /// Get order summary for date range
    /// </summary>
    Task<Result<OrderSummaryDto>> GetOrderSummaryAsync(DateTime dateFrom, DateTime dateTo);
}

/// <summary>
/// Cashback service interface
/// </summary>
public interface ICashbackService
{
    /// <summary>
    /// Get cashback summary for customer
    /// </summary>
    Task<Result<CashbackSummaryDto>> GetCashbackSummaryAsync(int customerId);
    
    /// <summary>
    /// Get cashback transaction history
    /// </summary>
    Task<Result<List<CashbackTransactionDto>>> GetCashbackHistoryAsync(
        int customerId, int pageNumber = 1, int pageSize = 20);
    
    /// <summary>
    /// Earn cashback from order (2%)
    /// </summary>
    Task<Result> EarnCashbackAsync(int customerId, int orderId, decimal orderAmount);
    
    /// <summary>
    /// Use cashback for order (FIFO)
    /// </summary>
    Task<Result> UseCashbackAsync(int customerId, int orderId, decimal amount);
    
    /// <summary>
    /// Expire old cashback transactions (background job)
    /// </summary>
    Task<Result> ExpireCashbackAsync();
    
    /// <summary>
    /// Get available cashback balance
    /// </summary>
    Task<Result<decimal>> GetAvailableCashbackAsync(int customerId);
    
    /// <summary>
    /// Get cashback expiring in next N days
    /// </summary>
    Task<Result<decimal>> GetExpiringCashbackAsync(int customerId, int daysThreshold = 7);
    
    /// <summary>
    /// Get customers with expiring cashback for notifications
    /// </summary>
    Task<Result<List<CashbackExpiryNotificationDto>>> GetCustomersWithExpiringCashbackAsync(
        int daysThreshold = 7);
}

/// <summary>
/// Cart service interface
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Get cart items for customer
    /// </summary>
    Task<Result<List<CartItemDto>>> GetCartItemsAsync(int customerId);
    
    /// <summary>
    /// Add item to cart
    /// </summary>
    Task<Result<CartItemDto>> AddToCartAsync(AddToCartDto request, int customerId);
    
    /// <summary>
    /// Update cart item quantity
    /// </summary>
    Task<Result<CartItemDto>> UpdateCartItemAsync(UpdateCartItemDto request, int customerId);
    
    /// <summary>
    /// Remove item from cart
    /// </summary>
    Task<Result> RemoveFromCartAsync(int cartItemId, int customerId);
    
    /// <summary>
    /// Clear all cart items
    /// </summary>
    Task<Result> ClearCartAsync(int customerId);
    
    /// <summary>
    /// Get cart total price
    /// </summary>
    Task<Result<decimal>> GetCartTotalAsync(int customerId);
    
    /// <summary>
    /// Get cart items count
    /// </summary>
    Task<Result<int>> GetCartItemsCountAsync(int customerId);
    
    /// <summary>
    /// Validate cart for checkout
    /// </summary>
    Task<Result> ValidateCartForCheckoutAsync(int customerId);
}
