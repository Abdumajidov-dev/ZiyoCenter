using System.Windows;

namespace ZiyoMarket.AdminPanel.Views;

public partial class RejectReasonDialog : Window
{
    public string Reason { get; private set; } = string.Empty;

    public RejectReasonDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => ReasonTextBox.Focus();
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ReasonTextBox.Text))
        {
            MessageBox.Show("Sabab kiritilmadi!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Reason = ReasonTextBox.Text.Trim();
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
