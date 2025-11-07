using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private ObservableCollection<Order> _orders = new();

    [ObservableProperty]
    private Order? _selectedOrder;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string? _selectedStatus;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount = 0;

    [ObservableProperty]
    private int _pageSize = 20;

    public List<string> OrderStatuses { get; } = new()
    {
        "Barchasi",
        "Pending",
        "Confirmed",
        "Preparing",
        "ReadyForPickup",
        "Shipped",
        "Delivered",
        "Cancelled"
    };

    public OrdersViewModel(IOrderService orderService)
    {
        _orderService = orderService;
        SelectedStatus = "Barchasi";
        _ = LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        IsLoading = true;
        try
        {
            var statusFilter = SelectedStatus == "Barchasi" ? null : SelectedStatus;
            var response = await _orderService.GetOrdersAsync(CurrentPage, PageSize, statusFilter, SearchText);

            if (response.Success && response.Data != null)
            {
                Orders.Clear();
                foreach (var order in response.Data.Items)
                {
                    Orders.Add(order);
                }

                TotalCount = response.Data.TotalCount;
                TotalPages = response.Data.TotalPages;
            }
            else
            {
                MessageBox.Show(response.Message ?? "Buyurtmalarni yuklashda xatolik!", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task FilterByStatusAsync()
    {
        CurrentPage = 1;
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private void ViewOrderDetails()
    {
        if (SelectedOrder == null)
        {
            MessageBox.Show("Iltimos, buyurtmani tanlang!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new Views.OrderDetailDialog(this);
        dialog.ShowDialog();
    }

    [RelayCommand]
    private async Task UpdateOrderStatusAsync(string newStatus)
    {
        if (SelectedOrder == null)
        {
            MessageBox.Show("Iltimos, buyurtmani tanlang!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Buyurtma #{SelectedOrder.OrderNumber} statusini {newStatus} ga o'zgartirmoqchimisiz?",
            "Tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            try
            {
                var dto = new UpdateOrderStatusDto
                {
                    OrderId = SelectedOrder.Id,
                    Status = newStatus
                };

                var response = await _orderService.UpdateOrderStatusAsync(dto);

                if (response.Success)
                {
                    MessageBox.Show("Buyurtma statusi muvaffaqiyatli yangilandi!", "Muvaffaqiyat",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadOrdersAsync();
                }
                else
                {
                    MessageBox.Show(response.Message ?? "Statusni yangilashda xatolik!", "Xatolik",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
