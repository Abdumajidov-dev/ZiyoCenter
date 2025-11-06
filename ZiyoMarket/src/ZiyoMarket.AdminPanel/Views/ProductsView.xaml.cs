using System.Windows.Controls;
using ZiyoMarket.AdminPanel.ViewModels;

namespace ZiyoMarket.AdminPanel.Views;

public partial class ProductsView : UserControl
{
    public ProductsView(ProductsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
