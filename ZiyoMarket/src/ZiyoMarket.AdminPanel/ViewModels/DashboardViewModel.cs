using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _currentPage = "Dashboard";

    [ObservableProperty]
    private object? _currentView;

    public DashboardViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private void NavigateToProducts()
    {
        CurrentPage = "Mahsulotlar";
        CurrentView = App.GetService<Views.ProductsView>();
    }

    [RelayCommand]
    private void NavigateToOrders()
    {
        CurrentPage = "Buyurtmalar";
        CurrentView = App.GetService<Views.OrdersView>();
    }

    [RelayCommand]
    private void NavigateToUsers()
    {
        CurrentPage = "Foydalanuvchilar";
        // TODO: Navigate to users view
    }

    [RelayCommand]
    private void NavigateToSupport()
    {
        CurrentPage = "Qo'llab-quvvatlash";
        // TODO: Navigate to support view
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = "Sozlamalar";
        // TODO: Navigate to settings view
    }

    [RelayCommand]
    private void Logout()
    {
        var result = MessageBox.Show("Tizimdan chiqishni xohlaysizmi?", "Tasdiqlash",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Login oynasini ochish
            var loginWindow = App.GetService<Views.LoginWindow>();
            loginWindow?.Show();

            // Dashboard oynasini yopish
            Application.Current.Windows.OfType<Views.DashboardWindow>().FirstOrDefault()?.Close();
        }
    }
}
