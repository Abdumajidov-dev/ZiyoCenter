using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Common;

namespace ZiyoMarket.Service.DTOs.Orders;

/// <summary>
/// Cart item DTO
/// </summary>
public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string FormattedTotalPrice => $"{TotalPrice:N0} so'm";
    public bool IsAvailable { get; set; }
    public int AvailableStock { get; set; }
    public DateTime AddedAt { get; set; }
}

/// <summary>
/// Add to cart DTO
/// </summary>
public class AddToCartDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid product")]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// Update cart item DTO
/// </summary>
public class UpdateCartItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CartItemId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

/// <summary>
/// Order list DTO
/// </summary>
public class OrderListDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? SellerId { get; set; }
    public string? SellerName { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal CashbackUsed { get; set; }
    public decimal FinalPrice { get; set; }
    public string FormattedFinalPrice => $"{FinalPrice:N0} so'm";
    public string PaymentMethod { get; set; } = string.Empty;
    public string DeliveryType { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
}

/// <summary>
/// Order detail DTO
/// </summary>
public class OrderDetailDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int? SellerId { get; set; }
    public string? SellerName { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // Pricing
    public decimal TotalPrice { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal CashbackUsed { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal FinalPrice { get; set; }
    public string FormattedFinalPrice => $"{FinalPrice:N0} so'm";
    
    // Payment
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Delivery
    public string DeliveryType { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    
    // Notes
    public string? CustomerNotes { get; set; }
    public string? SellerNotes { get; set; }
    public string? AdminNotes { get; set; }
    
    // Items
    public List<OrderItemDto> OrderItems { get; set; } = new();
    
    // Discounts
    public List<OrderDiscountDto> OrderDiscounts { get; set; } = new();
    
    // Business rules
    public bool CanBeCancelled { get; set; }
    public bool RequiresPayment { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Order item DTO
/// </summary>
public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountApplied { get; set; }
    public decimal TotalPrice { get; set; }
    public string FormattedTotalPrice => $"{TotalPrice:N0} so'm";
}

/// <summary>
/// Create order DTO
/// </summary>
public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public bool CreateFromCart { get; set; } = true;
    
    public List<CreateOrderItemDto>? Items { get; set; }
    
    [Required(ErrorMessage = "Payment method is required")]
    public string PaymentMethod { get; set; } = "Card"; // Cash, Card, Cashback, Mixed
    
    [Required(ErrorMessage = "Delivery type is required")]
    public string DeliveryType { get; set; } = "Pickup"; // Pickup, Postal, Courier
    
    public string? DeliveryAddress { get; set; }
    
    public int? DeliveryPartnerId { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal CashbackToUse { get; set; } = 0;
    
    [MaxLength(500)]
    public string? CustomerNotes { get; set; }
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; } = 0;  // ← QO'SHING!

    [MaxLength(500)]
    public string? SellerNotes { get; set; }
}

/// <summary>
/// Create order item DTO
/// </summary>
public class CreateOrderItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

/// <summary>
/// Update order status DTO
/// </summary>
public class UpdateOrderStatusDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int OrderId { get; set; }
    
    [Required]
    public string NewStatus { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Apply discount DTO
/// </summary>
public class ApplyDiscountDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int OrderId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal DiscountAmount { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int DiscountReasonId { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Order discount DTO
/// </summary>
public class OrderDiscountDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int DiscountReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string AppliedBy { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Discount reason DTO
/// </summary>
public class DiscountReasonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Order filter request
/// </summary>
public class OrderFilterRequest : BaseFilterRequest
{
    public int? CustomerId { get; set; }
    public int? SellerId { get; set; }
    public OrderStatus? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public string? DeliveryType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
}

/// <summary>
/// Order summary DTO
/// </summary>
public class OrderSummaryDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string FormattedTotalRevenue => $"{TotalRevenue:N0} so'm";
    public decimal AverageOrderValue { get; set; }
    public int PendingOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }

    public int OnlineOrders { get; set; }
    public int OfflineOrders { get; set; }
}
