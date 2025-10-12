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
    Task<r> CancelOrderAsync(int orderId, int userId, string userType, string? reason);
    
    // ============ Order Status Management ============
    
    /// <summary>
    /// Update order status
    /// </summary>
    Task<r> UpdateOrderStatusAsync(UpdateOrderStatusDto request, int updatedBy);
    
    /// <summary>
    /// Confirm order (by seller or admin)
    /// </summary>
    Task<r> ConfirmOrderAsync(int orderId, int sellerId);
    
    /// <summary>
    /// Mark order as ready for pickup
    /// </summary>
    Task<r> MarkAsReadyForPickupAsync(int orderId, int updatedBy);
    
    /// <summary>
    /// Mark order as shipped
    /// </summary>
    Task<r> MarkAsShippedAsync(int orderId, int updatedBy);
    
    /// <summary>
    /// Mark order as delivered
    /// </summary>
    Task<r> MarkAsDeliveredAsync(int orderId, int updatedBy);
    
    // ============ Discount Management ============
    
    /// <summary>
    /// Apply discount to order
    /// </summary>
    Task<r> ApplyDiscountAsync(ApplyDiscountDto request, int sellerId);
    
    /// <summary>
    /// Remove discount from order
    /// </summary>
    Task<r> RemoveDiscountAsync(int orderDiscountId, int sellerId);
    
    /// <summary>
    /// Get discount reasons
    /// </summary>
    Task<Result<List<DiscountReasonDto>>> GetDiscountReasonsAsync();
    
    // ============ Payment ============
    
    /// <summary>
    /// Process payment for order
    /// </summary>
    Task<r> ProcessPaymentAsync(int orderId, string paymentMethod, string? reference);
    
    /// <summary>
    /// Refund order
    /// </summary>
    Task<r> RefundOrderAsync(int orderId, int adminId, string reason);
    
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
    Task<r> EarnCashbackAsync(int customerId, int orderId, decimal orderAmount);
    
    /// <summary>
    /// Use cashback for order (FIFO)
    /// </summary>
    Task<r> UseCashbackAsync(int customerId, int orderId, decimal amount);
    
    /// <summary>
    /// Expire old cashback transactions (background job)
    /// </summary>
    Task<r> ExpireCashbackAsync();
    
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
    Task<r> RemoveFromCartAsync(int cartItemId, int customerId);
    
    /// <summary>
    /// Clear all cart items
    /// </summary>
    Task<r> ClearCartAsync(int customerId);
    
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
    Task<r> ValidateCartForCheckoutAsync(int customerId);
}
