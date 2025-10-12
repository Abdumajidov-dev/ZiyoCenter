using System;

namespace ZiyoMarket.Service.Exceptions;

/// <summary>
/// Base exception class for all custom exceptions
/// </summary>
public abstract class BaseException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    
    protected BaseException(string message, int statusCode = 400, string errorCode = "ERROR") 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
    
    protected BaseException(string message, Exception innerException, int statusCode = 400, string errorCode = "ERROR") 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception when entity is not found
/// </summary>
public class NotFoundException : BaseException
{
    public NotFoundException(string entityName, int id) 
        : base($"{entityName} with ID {id} not found", 404, "NOT_FOUND")
    {
    }
    
    public NotFoundException(string message) 
        : base(message, 404, "NOT_FOUND")
    {
    }
}

/// <summary>
/// Exception when user is not authorized
/// </summary>
public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access") 
        : base(message, 401, "UNAUTHORIZED")
    {
    }
}

/// <summary>
/// Exception when user doesn't have permission
/// </summary>
public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Access denied") 
        : base(message, 403, "FORBIDDEN")
    {
    }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
public class BusinessRuleException : BaseException
{
    public BusinessRuleException(string message) 
        : base(message, 400, "BUSINESS_RULE_VIOLATION")
    {
    }
}

/// <summary>
/// Exception when duplicate entity is found
/// </summary>
public class DuplicateException : BaseException
{
    public DuplicateException(string entityName, string field, object value) 
        : base($"{entityName} with {field} = '{value}' already exists", 409, "DUPLICATE")
    {
    }
}

/// <summary>
/// Exception when stock is insufficient
/// </summary>
public class InsufficientStockException : BaseException
{
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }
    
    public InsufficientStockException(string productName, int requestedQuantity, int availableQuantity) 
        : base($"Insufficient stock for {productName}. Requested: {requestedQuantity}, Available: {availableQuantity}", 
            400, "INSUFFICIENT_STOCK")
    {
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}

/// <summary>
/// Exception when cashback is insufficient
/// </summary>
public class InsufficientCashbackException : BaseException
{
    public decimal RequestedAmount { get; }
    public decimal AvailableAmount { get; }
    
    public InsufficientCashbackException(decimal requestedAmount, decimal availableAmount) 
        : base($"Insufficient cashback. Requested: {requestedAmount:C}, Available: {availableAmount:C}", 
            400, "INSUFFICIENT_CASHBACK")
    {
        RequestedAmount = requestedAmount;
        AvailableAmount = availableAmount;
    }
}

/// <summary>
/// Exception when order cannot be cancelled
/// </summary>
public class OrderCannotBeCancelledException : BaseException
{
    public OrderCannotBeCancelledException(int orderId, string currentStatus) 
        : base($"Order #{orderId} cannot be cancelled. Current status: {currentStatus}", 
            400, "ORDER_CANNOT_BE_CANCELLED")
    {
    }
}

/// <summary>
/// Exception when invalid payment method is used
/// </summary>
public class InvalidPaymentMethodException : BaseException
{
    public InvalidPaymentMethodException(string paymentMethod, string reason) 
        : base($"Invalid payment method '{paymentMethod}': {reason}", 
            400, "INVALID_PAYMENT_METHOD")
    {
    }
}

/// <summary>
/// Exception when discount exceeds allowed amount
/// </summary>
public class ExcessiveDiscountException : BaseException
{
    public decimal RequestedDiscount { get; }
    public decimal MaxAllowedDiscount { get; }
    
    public ExcessiveDiscountException(decimal requestedDiscount, decimal maxAllowedDiscount) 
        : base($"Discount amount {requestedDiscount:C} exceeds maximum allowed {maxAllowedDiscount:C}", 
            400, "EXCESSIVE_DISCOUNT")
    {
        RequestedDiscount = requestedDiscount;
        MaxAllowedDiscount = maxAllowedDiscount;
    }
}

/// <summary>
/// Exception when external service fails
/// </summary>
public class ExternalServiceException : BaseException
{
    public string ServiceName { get; }
    
    public ExternalServiceException(string serviceName, string message) 
        : base($"External service '{serviceName}' error: {message}", 
            503, "EXTERNAL_SERVICE_ERROR")
    {
        ServiceName = serviceName;
    }
    
    public ExternalServiceException(string serviceName, string message, Exception innerException) 
        : base($"External service '{serviceName}' error: {message}", innerException, 
            503, "EXTERNAL_SERVICE_ERROR")
    {
        ServiceName = serviceName;
    }
}
