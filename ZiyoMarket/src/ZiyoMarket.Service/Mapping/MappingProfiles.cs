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
using ZiyoMarket.Domain.Enums;

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
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailableForSale))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock))
            .ForMember(dest => dest.LikesCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore());
        
        CreateMap<Product, ProductDetailDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryPath, opt => opt.MapFrom(src => src.Category.GetFullPath()))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailableForSale))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock))
            .ForMember(dest => dest.IsOutOfStock, opt => opt.MapFrom(src => src.IsOutOfStock))
            .ForMember(dest => dest.LikesCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsLikedByCurrentUser, opt => opt.Ignore());
        
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProductStatus.Active));
        
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.QrCode, opt => opt.Ignore())
            .ForMember(dest => dest.StockQuantity, opt => opt.Ignore());
        
        CreateMap<Product, LowStockProductDto>();
        
        // Category mappings
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src =>
                src.Parent != null ? src.Parent.Name : null))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.DisplayOrder))
            .ForMember(dest => dest.ProductCount, opt => opt.Ignore());

        CreateMap<SaveCategoryDto, Category>()
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.SortOrder));
        
        // Cart mappings
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product.ImageUrl))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Product.IsAvailableForSale))
            .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.Product.StockQuantity));
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
        
        CreateMap<RegisterCustomerDto, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CashbackBalance, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
        
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

        CreateMap<RegisterSellerDto, Seller>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // Admin mappings
        CreateMap<Admin, UserProfileDto>()
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => "Admin"))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.CashbackBalance, opt => opt.MapFrom(src => 0));

        CreateMap<Admin, AdminListDto>();

        CreateMap<Admin, AdminDetailDto>();

        CreateMap<RegisterAdminDto, Admin>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
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
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product.ImageUrl));
        
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
            .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Parse(src.CreatedAt)));

        // SaveContentDto to Content entity
        CreateMap<SaveContentDto, Content>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.StartDate ?? DateTime.UtcNow))
            .ForMember(dest => dest.ValidUntil, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsPublished));
    }
}
