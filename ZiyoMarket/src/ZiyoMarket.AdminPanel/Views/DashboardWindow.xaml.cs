using System.Windows;
using ZiyoMarket.AdminPanel.ViewModels;

namespace ZiyoMarket.AdminPanel.Views;

public partial class DashboardWindow : Window
{
    public DashboardWindow(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
