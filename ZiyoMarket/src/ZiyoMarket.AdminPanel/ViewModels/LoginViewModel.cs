using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Telefon raqam va parol kiritilishi shart!";
            return;
        }

        IsLoading = true;

        try
        {
            var request = new LoginRequest
            {
                Phone = Phone,
                Password = Password,
                UserType = "Admin"
            };

            var response = await _apiService.LoginAsync(request);

            if (response.Success && response.Data != null)
            {
                // Token ni saqlash
                _apiService.SetAuthToken(response.Data.AccessToken);

                // Dashboard oynasini ochish
                var dashboard = App.GetService<Views.DashboardWindow>();
                dashboard?.Show();

                // Login oynasini yopish
                Application.Current.Windows.OfType<Views.LoginWindow>().FirstOrDefault()?.Close();
            }
            else
            {
                ErrorMessage = response.Message ?? "Login muvaffaqiyatsiz!";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Xatolik: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
