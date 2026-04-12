using System.Windows.Controls;
using ZiyoMarket.AdminPanel.ViewModels;

namespace ZiyoMarket.AdminPanel.Views;

public partial class NotificationsView : UserControl
{
    public NotificationsView(NotificationsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
