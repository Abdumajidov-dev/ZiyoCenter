using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;

    [ObservableProperty] private ObservableCollection<CustomerModel> _customers = new();
    [ObservableProperty] private CustomerModel? _selectedCustomer;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _pageSize = 20;
    [ObservableProperty] private string _statusFilter = "Barchasi";

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        _ = LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        IsLoading = true;
        try
        {
            bool? isActive = StatusFilter switch
            {
                "Faol" => true,
                "Faol emas" => false,
                _ => null
            };

            var response = await _customerService.GetCustomersAsync(CurrentPage, PageSize, SearchText, isActive);

            if (response.Success && response.Data != null)
            {
                Customers.Clear();
                foreach (var c in response.Data.Items)
                    Customers.Add(c);

                TotalCount = response.Data.TotalCount;
                TotalPages = response.Data.TotalPages;
            }
            else
            {
                MessageBox.Show(response.Message ?? "Mijozlarni yuklashda xatolik!", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task OpenDetailAsync(CustomerModel? customer)
    {
        if (customer == null) return;

        var detailResponse = await _customerService.GetCustomerByIdAsync(customer.Id);
        if (!detailResponse.Success || detailResponse.Data == null)
        {
            MessageBox.Show(detailResponse.Message ?? "Ma'lumot yuklanmadi", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var window = new Views.CustomerDetailWindow(detailResponse.Data, _customerService);
        window.ShowDialog();
        await LoadCustomersAsync();
    }

    partial void OnStatusFilterChanged(string value) => _ = SearchAsync();
}
