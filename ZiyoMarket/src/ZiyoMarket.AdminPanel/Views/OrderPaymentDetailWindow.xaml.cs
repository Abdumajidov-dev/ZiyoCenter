using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.Views;

public partial class OrderPaymentDetailWindow : Window
{
    private readonly INotificationsAdminService _service;
    private readonly AdminOrderModel _order;
    private string? _receiptUrl;

    public OrderPaymentDetailWindow(AdminOrderModel order, INotificationsAdminService service)
    {
        InitializeComponent();
        _service = service;
        _order = order;
        PopulateUI();
    }

    private void PopulateUI()
    {
        var o = _order;

        // Header
        HeaderOrderNumber.Text = $"Buyurtma {o.OrderNumber}";
        HeaderCustomerName.Text = $"Mijoz: {o.CustomerName}  ·  {o.CustomerPhone}";
        HeaderAmount.Text = o.FormattedPrice;

        // Customer info
        CustName.Text = o.CustomerName;
        CustPhone.Text = o.CustomerPhone;
        DeliveryType.Text = o.DeliveryType;
        DeliveryAddress.Text = string.IsNullOrWhiteSpace(o.DeliveryAddress) ? "Ko'rsatilmagan" : o.DeliveryAddress;
        CustomerNotes.Text = string.IsNullOrWhiteSpace(o.CustomerNotes) ? "—" : o.CustomerNotes;

        // Items
        ItemsGrid.ItemsSource = o.OrderItems;
        TotalAmount.Text = o.FormattedPrice;

        // Footer note
        FooterNote.Text = $"Buyurtma #{o.OrderNumber} · {o.CreatedAt}";

        // Receipt
        _receiptUrl = o.PaymentReceiptUrl;
        LoadReceipt(o.PaymentReceiptUrl);
    }

    private void LoadReceipt(string? url)
    {
        ReceiptUrlText.Text = url ?? "Chek yuklanmagan";

        if (string.IsNullOrEmpty(url))
        {
            NoReceiptPanel.Visibility = Visibility.Visible;
            ImageScrollViewer.Visibility = Visibility.Collapsed;
            PdfPanel.Visibility = Visibility.Collapsed;
            ApproveButton.IsEnabled = false;
            RejectButton.IsEnabled = false;
            return;
        }

        if (_order.IsImage)
        {
            // Load image from URL
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ReceiptImage.Source = bitmap;
                ImageScrollViewer.Visibility = Visibility.Visible;
                PdfPanel.Visibility = Visibility.Collapsed;
                NoReceiptPanel.Visibility = Visibility.Collapsed;
            }
            catch
            {
                // If URL is relative, try to handle gracefully
                NoReceiptPanel.Visibility = Visibility.Visible;
                ImageScrollViewer.Visibility = Visibility.Collapsed;
                ReceiptUrlText.Text = $"Rasmni yuklashda xatolik: {url}";
            }
        }
        else
        {
            // PDF or other file
            PdfPanel.Visibility = Visibility.Visible;
            ImageScrollViewer.Visibility = Visibility.Collapsed;
            NoReceiptPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void OpenPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_receiptUrl))
        {
            try { Process.Start(new ProcessStartInfo(_receiptUrl) { UseShellExecute = true }); }
            catch (Exception ex)
            {
                MessageBox.Show($"Faylni ochishda xatolik: {ex.Message}", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void ApproveButton_Click(object sender, RoutedEventArgs e)
    {
        var confirm = MessageBox.Show(
            $"Buyurtma {_order.OrderNumber} uchun to'lovni tasdiqlaysizmi?\n\n" +
            $"Summa: {_order.FormattedPrice}\n" +
            $"Mijoz: {_order.CustomerName}\n\n" +
            "Tasdiqlangandan keyin buyurtma 'Tasdiqlandi' holatiga o'tadi.",
            "To'lovni tasdiqlash",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        ApproveButton.IsEnabled = false;
        RejectButton.IsEnabled = false;

        try
        {
            var result = await _service.ApprovePaymentAsync(_order.Id);
            if (result.Success)
            {
                MessageBox.Show(
                    $"To'lov tasdiqlandi!\nBuyurtma {_order.OrderNumber} 'Tasdiqlandi' holatiga o'tdi.\n\n" +
                    "Endi mijoz bilan bog'lanib, yetkazib berish haqida kelishing mumkin.",
                    "Muvaffaqiyat", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(result.Message ?? "Xatolik yuz berdi", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ApproveButton.IsEnabled = true;
                RejectButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
            ApproveButton.IsEnabled = true;
            RejectButton.IsEnabled = true;
        }
    }

    private async void RejectButton_Click(object sender, RoutedEventArgs e)
    {
        var reasonWindow = new RejectReasonDialog();
        if (reasonWindow.ShowDialog() != true || string.IsNullOrWhiteSpace(reasonWindow.Reason))
            return;

        ApproveButton.IsEnabled = false;
        RejectButton.IsEnabled = false;

        try
        {
            var result = await _service.RejectPaymentAsync(_order.Id, reasonWindow.Reason);
            if (result.Success)
            {
                MessageBox.Show(
                    "To'lov rad etildi.\nMijoz buyurtmasi 'Kutilmoqda' holatiga qaytdi.\n" +
                    "Mijoz chekni qayta yuborishi kerak bo'ladi.",
                    "Rad etildi", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = false;
                Close();
            }
            else
            {
                MessageBox.Show(result.Message ?? "Xatolik yuz berdi", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ApproveButton.IsEnabled = true;
                RejectButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
            ApproveButton.IsEnabled = true;
            RejectButton.IsEnabled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
