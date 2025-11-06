using System.Windows.Controls;
using ZiyoMarket.AdminPanel.ViewModels;

namespace ZiyoMarket.AdminPanel.Views;

public partial class OrdersView : UserControl
{
    public OrdersView(OrdersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
