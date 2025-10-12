using AutoMapper;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Domain.Entities.Users;
using ZiyoMarket.Domain.Entities.Orders;
using ZiyoMarket.Service.DTOs.Products;
using ZiyoMarket.Service.DTOs.Auth;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.DTOs.Customers;
using ZiyoMarket.Service.DTOs.Cashback;
using ZiyoMarket.Domain.Enums;

namespace ZiyoMarket.Service.Mapping;

/// <summary>
/// Product mapping profile
/// </summary>
public class ProductProfile : Profile
{
    public ProductProfile()
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
            .ForMember(dest => dest.ProductCount, opt => opt.Ignore());
        
        CreateMap<SaveCategoryDto, Category>();
        
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
        
        // Admin mappings
        CreateMap<Admin, UserProfileDto>()
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => "Admin"))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.Address, opt => opt.Ignore())
            .ForMember(dest => dest.CashbackBalance, opt => opt.MapFrom(src => 0));
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
