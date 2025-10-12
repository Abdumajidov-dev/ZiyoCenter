using FluentValidation;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.DTOs.Orders;

namespace ZiyoMarket.Service.Validators;

/// <summary>
/// Login request validator
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.PhoneOrEmail)
            .NotEmpty().WithMessage("Phone/Email/Username is required")
            .MaximumLength(255).WithMessage("Must not exceed 255 characters");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        
        RuleFor(x => x.UserType)
            .NotEmpty().WithMessage("User type is required")
            .Must(x => new[] { "Customer", "Seller", "Admin" }.Contains(x))
            .WithMessage("Invalid user type");
    }
}

/// <summary>
/// Register customer validator
/// </summary>
public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerDto>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
        
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^(\+?998)?[0-9]{9}$")
            .WithMessage("Invalid phone number format");
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}

/// <summary>
/// Create product validator
/// </summary>
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(300).WithMessage("Name must not exceed 300 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
        
        RuleFor(x => x.QRCode)
            .NotEmpty().WithMessage("QR code is required")
            .MaximumLength(100).WithMessage("QR code must not exceed 100 characters");
        
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Price is too large");
        
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");
        
        RuleFor(x => x.MinStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Min stock level cannot be negative");
    }
}

/// <summary>
/// Update product validator
/// </summary>
public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Valid product ID is required");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(300).WithMessage("Name must not exceed 300 characters");
        
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required");
        
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Price is too large");
    }
}

/// <summary>
/// Create order validator
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .When(x => !x.CreateFromCart);
        
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(x => new[] { "Cash", "Card", "Cashback", "Mixed" }.Contains(x))
            .WithMessage("Invalid payment method");
        
        RuleFor(x => x.DeliveryType)
            .NotEmpty().WithMessage("Delivery type is required")
            .Must(x => new[] { "Pickup", "Postal", "Courier" }.Contains(x))
            .WithMessage("Invalid delivery type");
        
        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required")
            .MaximumLength(500).WithMessage("Delivery address must not exceed 500 characters")
            .When(x => x.DeliveryType != "Pickup");
        
        RuleFor(x => x.DeliveryPartnerId)
            .GreaterThan(0).WithMessage("Delivery partner is required")
            .When(x => x.DeliveryType != "Pickup");
        
        RuleFor(x => x.CashbackToUse)
            .GreaterThanOrEqualTo(0).WithMessage("Cashback amount cannot be negative");
    }
}

/// <summary>
/// Update stock validator
/// </summary>
public class UpdateStockValidator : AbstractValidator<UpdateStockDto>
{
    public UpdateStockValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product ID is required");
        
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        
        RuleFor(x => x.Operation)
            .NotEmpty().WithMessage("Operation is required")
            .Must(x => new[] { "add", "remove", "set" }.Contains(x.ToLower()))
            .WithMessage("Invalid operation. Must be 'add', 'remove', or 'set'");
    }
}

/// <summary>
/// Apply discount validator
/// </summary>
public class ApplyDiscountValidator : AbstractValidator<ApplyDiscountDto>
{
    public ApplyDiscountValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Valid order ID is required");
        
        RuleFor(x => x.DiscountAmount)
            .GreaterThan(0).WithMessage("Discount amount must be greater than 0");
        
        RuleFor(x => x.DiscountReasonId)
            .GreaterThan(0).WithMessage("Discount reason is required");
    }
}
