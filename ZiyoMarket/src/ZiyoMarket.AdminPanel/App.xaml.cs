using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ZiyoMarket.AdminPanel.Services;
using ZiyoMarket.AdminPanel.ViewModels;
using ZiyoMarket.AdminPanel.Views;

namespace ZiyoMarket.AdminPanel;

public partial class App : Application
{
    private static ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Open LoginWindow
        var loginWindow = _serviceProvider?.GetService<LoginWindow>();
        loginWindow?.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // HTTP Client
        services.AddHttpClient<IApiService, ApiService>();

        // Services
        services.AddTransient<IProductService, ProductService>();
        services.AddTransient<IOrderService, OrderService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<OrdersViewModel>();

        // Views
        services.AddTransient<LoginWindow>();
        services.AddTransient<DashboardWindow>();
        services.AddTransient<ProductsView>();
        services.AddTransient<OrdersView>();
    }

    public static T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }
}

