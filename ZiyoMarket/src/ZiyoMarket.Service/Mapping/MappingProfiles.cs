using AutoMapper;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Domain.Entities.Notifications;
using ZiyoMarket.Domain.Entities.Delivery;
using ZiyoMarket.Domain.Entities.Content;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.DTOs.Customers;
using ZiyoMarket.Service.DTOs.Cashback;
using ZiyoMarket.Service.DTOs.Notifications;
using ZiyoMarket.Service.DTOs.Delivery;
using ZiyoMarket.Service.DTOs.Admins;
using ZiyoMarket.Service.DTOs.Sellers;
using ZiyoMarket.Service.DTOs.Content;
using ZiyoMarket.Service.DTOs.Sms;
using ZiyoMarket.Service.DTOs.Payment;
using ZiyoMarket.Service.DTOs.Update;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Domain.Entities.Systems;

namespace ZiyoMarket.Service.Mapping;

/// <summary>
/// Product mapping profile
/// </summary>
public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Product mappings
        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.CategoryId).ToList()))
            .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.Category.Name).ToList()))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailableForSale))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock))
            .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore())
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()));

        
        CreateMap<Product, ProductDetailDto>()
            .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.CategoryId).ToList()))
            .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.Category.Name).ToList()))
            .ForMember(dest => dest.CategoryPaths, opt => opt.MapFrom(src => src.ProductCategories.Select(pc => pc.Category.GetFullPath()).ToList()))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailableForSale))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock))
            .ForMember(dest => dest.IsOutOfStock, opt => opt.MapFrom(src => src.IsOutOfStock))
            .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore())
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()));

        
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.QrCode, opt => opt.MapFrom(src => src.QRCode))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProductStatus.Active));
        
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.QrCode, opt => opt.MapFrom(src => src.QRCode))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.StockQuantity, opt => opt.Ignore());
        
        CreateMap<Product, LowStockProductDto>();
        
        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src =>
                src.Parent != null ? src.Parent.Name : null))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src =>
                src.ProductCategories != null ? src.ProductCategories.Count(pc => pc.Product != null && pc.Product.IsActive && pc.Product.DeletedAt == null) : 0))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
                ParseCreatedAt(src.CreatedAt)))
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));


        CreateMap<SaveCategoryDto, Category>()
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.SortOrder));
    }

    private static DateTime ParseCreatedAt(string? createdAt)
    {
        if (string.IsNullOrEmpty(createdAt))
            return DateTime.UtcNow;

        return DateTime.TryParse(createdAt, out var date) ? date : DateTime.UtcNow;
    }
}

/// <summary>
/// Cart mapping profile
/// </summary>
public class CartProfile : Profile
{
    public CartProfile()
    {
        // Cart mappings
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Noma'lum mahsulot"))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null && src.Product.Images.Any() ? src.Product.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl ?? src.Product.Images.First().ImageUrl : null))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Product != null && src.Product.IsAvailableForSale))
            .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.Product != null ? src.Product.StockQuantity : 0));
    }
}

/// <summary>
/// User mapping profile
/// </summary>
public class UserProfile : Profile
{
    public UserProfile()
    {
        // Customer mappings
        CreateMap<Customer, UserProfileDto>()
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => "Customer"));
        
        CreateMap<Customer, CustomerListDto>()
            .ForMember(dest => dest.OrdersCount, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSpent, opt => opt.Ignore());
        
        CreateMap<Customer, CustomerDetailDto>()
            .ForMember(dest => dest.TotalOrders, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSpent, opt => opt.Ignore())
            .ForMember(dest => dest.TotalCashbackEarned, opt => opt.Ignore())
            .ForMember(dest => dest.TotalCashbackUsed, opt => opt.Ignore())
            .ForMember(dest => dest.LastOrderDate, opt => opt.Ignore());
        

        
        CreateMap<UpdateCustomerDto, Customer>()
            .ForMember(dest => dest.Phone, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        
        // Seller mappings
        CreateMap<Seller, UserProfileDto>()
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => "Seller"))
            .ForMember(dest => dest.Address, opt => opt.Ignore())
            .ForMember(dest => dest.CashbackBalance, opt => opt.MapFrom(src => 0));

        CreateMap<Seller, SellerListDto>()
            .ForMember(dest => dest.TotalOrders, opt => opt.Ignore());

        CreateMap<Seller, SellerDetailDto>()
            .ForMember(dest => dest.TotalOrders, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSales, opt => opt.Ignore());



        // Admin mappings
        CreateMap<Admin, UserProfileDto>()
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => "Admin"))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.CashbackBalance, opt => opt.MapFrom(src => 0));

        CreateMap<Admin, AdminListDto>();

        CreateMap<Admin, AdminDetailDto>();


    }
}

/// <summary>
/// Order mapping profile
/// </summary>
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // Order mappings
        CreateMap<Order, OrderListDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => 
                $"{src.Customer.FirstName} {src.Customer.LastName}"))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : null))
            .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.OrderItems.Count));

        CreateMap<Order, OrderDetailDto>()
     .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
         $"{src.Customer.FirstName} {src.Customer.LastName}"))
     .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.Phone))
     .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src =>
         src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : null))
     .ForMember(dest => dest.CanBeCancelled, opt => opt.MapFrom(src => src.CanBeCancelled))
     .ForMember(dest => dest.RequiresPayment, opt => opt.MapFrom(src => src.RequiresPayment));



        // Order item mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null && src.Product.Images.Any() ? src.Product.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl ?? src.Product.Images.First().ImageUrl : null));
        
        // Order discount mappings
        CreateMap<OrderDiscount, OrderDiscountDto>()
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src => src.DiscountReason.Name))
            .ForMember(dest => dest.AppliedBy, opt => opt.MapFrom(src =>
                                src.AppliedBySeller != null
                                    ? $"{src.AppliedBySeller.FirstName} {src.AppliedBySeller.LastName}"
                                    : "Tizim tomonidan"))


            .ForMember(dest => dest.AppliedAt, opt => opt.MapFrom(src => 
                DateTime.Parse(src.CreatedAt)));
        
        // Discount reason mappings
        CreateMap<DiscountReason, DiscountReasonDto>();
    }
}

/// <summary>
/// Cashback mapping profile
/// </summary>
public class CashbackProfile : Profile
{
    public CashbackProfile()
    {
        CreateMap<CashbackTransaction, CashbackTransactionDto>();
    }
}

/// <summary>
/// Notification mapping profile
/// </summary>
public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.NotificationType.ToString()))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserType.ToString()))
            .ForMember(dest => dest.PushSent, opt => opt.MapFrom(src => src.IsPushSent))
            .ForMember(dest => dest.EmailSent, opt => opt.MapFrom(src => src.IsEmailSent))
            .ForMember(dest => dest.SMSSent, opt => opt.MapFrom(src => src.IsSmsSent))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Parse(src.CreatedAt)));
    }
}

/// <summary>
/// Delivery mapping profile
/// </summary>
public class DeliveryProfile : Profile
{
    public DeliveryProfile()
    {
        CreateMap<DeliveryPartner, DeliveryPartnerDto>()
            .ForMember(dest => dest.TotalDeliveries, opt => opt.Ignore())
            .ForMember(dest => dest.SuccessRate, opt => opt.Ignore());

        CreateMap<DeliveryPartner, DeliveryPartnerDetailDto>()
            .ForMember(dest => dest.TotalDeliveries, opt => opt.Ignore())
            .ForMember(dest => dest.SuccessRate, opt => opt.Ignore())
            .ForMember(dest => dest.AverageDeliveryDays, opt => opt.Ignore());

        CreateMap<OrderDelivery, OrderDeliveryDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryPartnerName, opt => opt.Ignore())
            .ForMember(dest => dest.IsDelayed, opt => opt.MapFrom(src => src.IsDelayed));
    }
}

/// <summary>
/// Content mapping profile
/// </summary>
public class ContentProfile : Profile
{
    public ContentProfile()
    {
        // Content entity to DTOs
        CreateMap<Content, ContentDetailDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ContentType.ToString()))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ContentUrl))
            .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.ContentUrl))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.ContentData))
            .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => src.VideoDuration))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.ValidUntil))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Parse(src.CreatedAt)))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.UpdatedAt) ? (DateTime?)DateTime.Parse(src.UpdatedAt) : null));

        CreateMap<Content, ContentListDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ContentType.ToString()))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.ValidUntil))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ContentUrl))
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Parse(src.CreatedAt)));

        // SaveContentDto to Content entity
        CreateMap<SaveContentDto, Content>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.ContentUrl, opt => opt.MapFrom(src => src.ImageUrl ?? src.VideoUrl))
            .ForMember(dest => dest.ContentData, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.VideoDuration, opt => opt.MapFrom(src => src.DurationSeconds))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => (src.StartDate ?? DateTime.UtcNow).ToUniversalTime()))
            .ForMember(dest => dest.ValidUntil, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToUniversalTime() : (DateTime?)null))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsPublished));
    }
}

/// <summary>
/// SMS mapping profile
/// </summary>
public class SmsProfile : Profile
{
    public SmsProfile()
    {
        // SmsLog to SmsLogDto
        CreateMap<SmsLog, SmsLogDto>()
            .ForMember(dest => dest.Purpose, opt => opt.MapFrom(src => src.Purpose.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}

/// <summary>
/// Payment mapping profile
/// </summary>
public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        // PaymentProof to PaymentProofResultDto
        CreateMap<PaymentProof, PaymentProofResultDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : ""))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Order != null && src.Order.Customer != null ? src.Order.Customer.FullName : ""))
            .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Order != null && src.Order.Customer != null ? src.Order.Customer.Phone : ""))
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.GetStatusText()))
            .ForMember(dest => dest.ReviewedByName, opt => opt.Ignore()); // Set manually in controller

        // SubmitPaymentProofDto to PaymentProof
        CreateMap<SubmitPaymentProofDto, PaymentProof>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => PaymentMethod.BankTransfer))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PaymentStatus.UnderReview))
            .ForMember(dest => dest.ProofImageUrl, opt => opt.Ignore());
    }
}


/// <summary>
/// Auto-Update mapping profile
/// </summary>
public class UpdateProfile : Profile
{
    public UpdateProfile()
    {
        CreateMap<AppVersion, AppVersionDto>()
            .ForMember(dest => dest.DownloadCount, opt => opt.Ignore()); // Set manually

        CreateMap<AppVersion, UpdateInfoDto>()
            .ForMember(dest => dest.UpdateAvailable, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentVersion, opt => opt.Ignore());
    }
}
