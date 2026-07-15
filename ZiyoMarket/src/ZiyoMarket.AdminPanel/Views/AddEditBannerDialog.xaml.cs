using System.Windows;
using ZiyoMarket.AdminPanel.ViewModels;

namespace ZiyoMarket.AdminPanel.Views;

public partial class AddEditBannerDialog : Window
{
    public AddEditBannerDialog(BannersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
