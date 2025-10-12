using ZiyoMarket.Data.Context;
using ZiyoMarket.Data.Interfaces;
using ZiyoMarket.Data.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;



namespace ZiyoMarket.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ZiyoMarketDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


        // Unit of Work
       // services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
