using System.Windows;
using System.Windows.Media;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.Views;

public partial class CustomerDetailWindow : Window
{
    private readonly ICustomerService _customerService;
    private CustomerDetailModel _customer;

    public CustomerDetailWindow(CustomerDetailModel customer, ICustomerService customerService)
    {
        InitializeComponent();
        _customerService = customerService;
        _customer = customer;
        PopulateUI();
    }

    private void PopulateUI()
    {
        var c = _customer;

        // Header
        InitialsText.Text = c.Initials;
        FullNameText.Text = c.FullName;
        PhoneText.Text = c.Phone;
        JoinDateText.Text = $"A'zo: {FormatDate(c.CreatedAt)}";

        // Status badge in header
        UpdateStatusBadge(c.IsActive);

        // Stats
        TotalOrdersText.Text = c.TotalOrders.ToString();
        TotalSpentText.Text = c.FormattedTotalSpent;
        CashbackText.Text = c.FormattedCashback;
        CashbackEarnedText.Text = c.FormattedCashbackEarned;

        // Personal info
        DetailFullName.Text = c.FullName;
        DetailPhone.Text = c.Phone;
        DetailAddress.Text = string.IsNullOrWhiteSpace(c.Address) ? "Ko'rsatilmagan" : c.Address;
        LastOrderText.Text = string.IsNullOrWhiteSpace(c.LastOrderDate) ? "Hali buyurtma yo'q" : FormatDate(c.LastOrderDate);
        UpdatedAtText.Text = FormatDate(c.UpdatedAt);

        // Avatar card
        BigInitialsText.Text = c.Initials;
        AvatarNameText.Text = c.FullName;
        AvatarPhoneText.Text = c.Phone;
        UpdateAvatarStatus(c.IsActive);

        // Footer info
        FooterInfoText.Text = $"ID: {c.Id} · Cashback sarfladi: {c.FormattedCashbackUsed}";

        // Toggle button state
        UpdateToggleButton(c.IsActive);
    }

    private void UpdateStatusBadge(bool isActive)
    {
        if (isActive)
        {
            StatusBadge.Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255));
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x69, 0xF0, 0xAE));
            StatusText.Text = "Faol";
        }
        else
        {
            StatusBadge.Background = new SolidColorBrush(Color.FromArgb(80, 255, 80, 80));
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x53, 0x52));
            StatusText.Text = "Faol emas";
        }
    }

    private void UpdateAvatarStatus(bool isActive)
    {
        if (isActive)
        {
            AvatarStatusBadge.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
            AvatarStatusText.Text = "● Faol";
        }
        else
        {
            AvatarStatusBadge.Background = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
            AvatarStatusText.Text = "● Faol emas";
        }
    }

    private void UpdateToggleButton(bool isActive)
    {
        if (isActive)
        {
            ToggleStatusText.Text = "Bloklash";
            ToggleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountCancel;
            ToggleStatusButton.Foreground = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
            ToggleStatusButton.BorderBrush = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
        }
        else
        {
            ToggleStatusText.Text = "Faollashtirish";
            ToggleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountCheck;
            ToggleStatusButton.Foreground = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
            ToggleStatusButton.BorderBrush = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
        }
    }

    private static string FormatDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return "—";
        if (DateTime.TryParse(dateStr, out var dt))
            return dt.ToString("dd.MM.yyyy HH:mm");
        return dateStr;
    }

    private async void ToggleStatusButton_Click(object sender, RoutedEventArgs e)
    {
        var action = _customer.IsActive ? "bloklash" : "faollashtirish";
        var confirm = MessageBox.Show(
            $"{_customer.FullName} ni {action}ni xohlaysizmi?",
            "Tasdiqlash", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        ToggleStatusButton.IsEnabled = false;
        try
        {
            var result = await _customerService.ToggleStatusAsync(_customer.Id);
            if (result.Success)
            {
                _customer.IsActive = !_customer.IsActive;
                UpdateStatusBadge(_customer.IsActive);
                UpdateAvatarStatus(_customer.IsActive);
                UpdateToggleButton(_customer.IsActive);
                MessageBox.Show("Holat muvaffaqiyatli o'zgartirildi!", "Muvaffaqiyat",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message ?? "Xatolik yuz berdi", "Xatolik",
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
            ToggleStatusButton.IsEnabled = true;
        }
    }

    private async void PromoteButton_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            $"{_customer.FullName} ni Admin roliga ko'tarmoqchimisiz?\n\n" +
            "Bu foydalanuvchi tizimga Admin sifatida kirish huquqiga ega bo'ladi.",
            "Admin qilish — Tasdiqlash",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        PromoteButton.IsEnabled = false;
        try
        {
            var result = await _customerService.PromoteToAdminAsync(_customer.Id, "Admin");
            if (result.Success)
            {
                MessageBox.Show(
                    $"{_customer.FullName} muvaffaqiyatli Admin qilindi!\nEndi u Admin sifatida tizimga kirishi mumkin.",
                    "Muvaffaqiyat", MessageBoxButton.OK, MessageBoxImage.Information);
                PromoteButton.IsEnabled = false;
                PromoteButton.Content = "✓ Admin qilindi";
            }
            else
            {
                MessageBox.Show(result.Message ?? "Xatolik yuz berdi", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                PromoteButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
            PromoteButton.IsEnabled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
