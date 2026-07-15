using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.ViewModels;

public partial class BannersViewModel : ObservableObject
{
    private readonly IBannerService _bannerService;
    private Window? _activeDialog;

    [ObservableProperty]
    private ObservableCollection<BannerModel> _banners = new();

    [ObservableProperty]
    private BannerModel? _selectedBanner;

    [ObservableProperty]
    private bool _isLoading;

    // Form fields for Add/Edit
    [ObservableProperty]
    private string _formTitle = string.Empty;

    [ObservableProperty]
    private string _formDescription = string.Empty;

    [ObservableProperty]
    private string _formImageUrl = string.Empty;

    [ObservableProperty]
    private string _formExternalUrl = string.Empty;

    [ObservableProperty]
    private bool _formIsPublished = true;

    [ObservableProperty]
    private int _formSortOrder;

    [ObservableProperty]
    private string _formTargetAudience = "All";

    [ObservableProperty]
    private DateTime? _formStartDate;

    [ObservableProperty]
    private DateTime? _formEndDate;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int _editingBannerId;

    public List<string> TargetAudiences { get; } = new() { "All", "Customers", "Sellers", "Admins" };

    public BannersViewModel(IBannerService bannerService)
    {
        _bannerService = bannerService;
        _ = LoadBannersAsync();
    }

    [RelayCommand]
    private async Task LoadBannersAsync()
    {
        IsLoading = true;
        try
        {
            var response = await _bannerService.GetBannersAsync();

            if (response.Success && response.Data != null)
            {
                Banners.Clear();
                foreach (var banner in response.Data.OrderBy(b => b.SortOrder))
                {
                    Banners.Add(banner);
                }
            }
            else
            {
                MessageBox.Show(response.Message ?? "Bannerlarni yuklashda xatolik!", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenAddDialog()
    {
        ClearForm();
        IsEditMode = false;

        _activeDialog = new Views.AddEditBannerDialog(this);
        _activeDialog.ShowDialog();
    }

    [RelayCommand]
    private void OpenEditDialog()
    {
        if (SelectedBanner == null)
        {
            MessageBox.Show("Iltimos, bannerni tanlang!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsEditMode = true;
        EditingBannerId = SelectedBanner.Id;
        FormTitle = SelectedBanner.Title;
        FormDescription = SelectedBanner.Description ?? string.Empty;
        FormImageUrl = SelectedBanner.ImageUrl ?? string.Empty;
        FormExternalUrl = SelectedBanner.ExternalUrl ?? string.Empty;
        FormIsPublished = SelectedBanner.IsPublished;
        FormSortOrder = SelectedBanner.SortOrder;
        FormTargetAudience = SelectedBanner.TargetAudience;
        FormStartDate = SelectedBanner.StartDate;
        FormEndDate = SelectedBanner.EndDate;

        _activeDialog = new Views.AddEditBannerDialog(this);
        _activeDialog.ShowDialog();
    }

    [RelayCommand]
    private async Task SaveBannerAsync()
    {
        if (string.IsNullOrWhiteSpace(FormTitle) || string.IsNullOrWhiteSpace(FormImageUrl))
        {
            MessageBox.Show("Iltimos, sarlavha va rasm URL'ini kiriting!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var dto = new SaveBannerDto
            {
                Title = FormTitle,
                Description = FormDescription,
                ImageUrl = FormImageUrl,
                ExternalUrl = FormExternalUrl,
                IsPublished = FormIsPublished,
                SortOrder = FormSortOrder,
                TargetAudience = FormTargetAudience,
                StartDate = FormStartDate,
                EndDate = FormEndDate
            };

            var response = IsEditMode
                ? await _bannerService.UpdateBannerAsync(EditingBannerId, dto)
                : await _bannerService.CreateBannerAsync(dto);

            if (response.Success)
            {
                MessageBox.Show(IsEditMode ? "Banner muvaffaqiyatli yangilandi!" : "Banner muvaffaqiyatli qo'shildi!",
                    "Muvaffaqiyat", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadBannersAsync();
                ClearForm();
                CloseDialog();
            }
            else
            {
                MessageBox.Show(response.Message ?? "Bannerni saqlashda xatolik!", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteBannerAsync()
    {
        if (SelectedBanner == null)
        {
            MessageBox.Show("Iltimos, bannerni tanlang!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"\"{SelectedBanner.Title}\" bannerini o'chirmoqchimisiz?",
            "Tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            try
            {
                var response = await _bannerService.DeleteBannerAsync(SelectedBanner.Id);

                if (response.Success)
                {
                    MessageBox.Show("Banner muvaffaqiyatli o'chirildi!", "Muvaffaqiyat",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadBannersAsync();
                }
                else
                {
                    MessageBox.Show(response.Message ?? "Bannerni o'chirishda xatolik!", "Xatolik",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xatolik: {ex.Message}", "Xatolik",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    private void ClearForm()
    {
        FormTitle = string.Empty;
        FormDescription = string.Empty;
        FormImageUrl = string.Empty;
        FormExternalUrl = string.Empty;
        FormIsPublished = true;
        FormSortOrder = 0;
        FormTargetAudience = "All";
        FormStartDate = null;
        FormEndDate = null;
    }

    private void CloseDialog()
    {
        _activeDialog?.Close();
        _activeDialog = null;
    }
}
