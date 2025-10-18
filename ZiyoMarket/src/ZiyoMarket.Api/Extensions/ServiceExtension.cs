using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ZiyoMarket.Data.IRepositories;
using ZiyoMarket.Data.Repositories;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Mapping;
using ZiyoMarket.Service.Services;

namespace ZiyoMarket.Api.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // ========== AutoMapper ==========
        // Automatically loads all mapping profiles from the Service layer assembly
        services.AddAutoMapper(typeof(MappingProfiles).Assembly);

        // ========== Generic Repository ==========
        // Registers generic repository pattern for all entities
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // ========== Unit of Work ==========
        // Transaction management and coordinated repository access
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ========== Authentication & Authorization ==========
        services.AddScoped<IAuthService, AuthService>();

        // ========== Product Management ==========
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        // ========== Shopping Cart ==========
        services.AddScoped<ICartService, CartService>();

        // ========== Order Management ==========
        services.AddScoped<IOrderService, OrderService>();

        // ========== User Management ==========
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISellerService, SellerService>();

        // ========== Financial Services ==========
        services.AddScoped<ICashbackService, CashbackService>();

        // ========== Delivery Services ==========
        services.AddScoped<IDeliveryService, DeliveryService>();

        // ========== Notification Services ==========
        services.AddScoped<INotificationService, NotificationService>();

        // ========== Support & Chat ==========
        services.AddScoped<ISupportService, SupportService>();

        // ========== Content Management ==========
        services.AddScoped<IContentService, ContentService>();

        // ========== Reporting & Analytics ==========
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
