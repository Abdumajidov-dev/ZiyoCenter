using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.ViewModels;

public partial class NotificationsViewModel : ObservableObject
{
    private readonly INotificationsAdminService _service;

    [ObservableProperty] private ObservableCollection<NotificationModel> _notifications = new();
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _unreadCount;
    [ObservableProperty] private bool _showOnlyPayments = true;

    public NotificationsViewModel(INotificationsAdminService service)
    {
        _service = service;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var response = await _service.GetNotificationsAsync(1, 50);
            if (response.Success && response.Data != null)
            {
                Notifications.Clear();
                var items = ShowOnlyPayments
                    ? response.Data.Where(n => n.IsPaymentReceipt)
                    : response.Data;

                foreach (var n in items.OrderByDescending(n => n.CreatedAt))
                    Notifications.Add(n);

                UnreadCount = Notifications.Count(n => !n.IsRead);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Bildirishnomalarni yuklashda xatolik: {ex.Message}",
                "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenOrderAsync(NotificationModel? notification)
    {
        if (notification == null) return;

        var data = notification.ParsedData;
        if (data == null)
        {
            MessageBox.Show("Buyurtma ma'lumotlari topilmadi", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Mark as read
        if (!notification.IsRead)
            await _service.MarkAsReadAsync(notification.Id);

        var orderResponse = await _service.GetOrderDetailAsync(data.OrderId);
        if (!orderResponse.Success || orderResponse.Data == null)
        {
            MessageBox.Show(orderResponse.Message ?? "Buyurtma topilmadi", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var window = new Views.OrderPaymentDetailWindow(orderResponse.Data, _service);
        window.ShowDialog();
        await LoadAsync();
    }

    partial void OnShowOnlyPaymentsChanged(bool value) => _ = LoadAsync();
}
