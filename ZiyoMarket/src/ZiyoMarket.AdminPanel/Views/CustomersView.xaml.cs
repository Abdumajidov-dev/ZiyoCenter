using System.Windows.Controls;
using ZiyoMarket.AdminPanel.ViewModels;

namespace ZiyoMarket.AdminPanel.Views;

public partial class CustomersView : UserControl
{
    public CustomersView(CustomersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
