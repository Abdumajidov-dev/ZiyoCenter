using System.Windows.Controls;
using ZiyoMarket.AdminPanel.ViewModels;

namespace ZiyoMarket.AdminPanel.Views;

public partial class BannersView : UserControl
{
    public BannersView(BannersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
