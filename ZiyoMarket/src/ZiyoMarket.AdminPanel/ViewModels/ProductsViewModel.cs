using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZiyoMarket.AdminPanel.Models;
using ZiyoMarket.AdminPanel.Services;

namespace ZiyoMarket.AdminPanel.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private Window? _activeDialog;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount = 0;

    [ObservableProperty]
    private int _pageSize = 20;

    // Form fields for Add/Edit
    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private string _formDescription = string.Empty;

    [ObservableProperty]
    private string _formQrCode = string.Empty;

    [ObservableProperty]
    private decimal _formPrice;

    [ObservableProperty]
    private int _formStockQuantity;

    [ObservableProperty]
    private int _formCategoryId;

    [ObservableProperty]
    private string _formImageUrl = string.Empty;

    [ObservableProperty]
    private int _formMinStockLevel = 10;

    [ObservableProperty]
    private string _formWeight = string.Empty;

    [ObservableProperty]
    private string _formDimensions = string.Empty;

    [ObservableProperty]
    private string _formManufacturer = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private int _editingProductId;

    public ProductsViewModel(IProductService productService)
    {
        _productService = productService;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        await LoadProductsAsync();
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            var response = await _productService.GetProductsAsync(CurrentPage, PageSize, SearchText);

            if (response.Success && response.Data != null)
            {
                Products.Clear();
                foreach (var product in response.Data.Items)
                {
                    Products.Add(product);
                }

                TotalCount = response.Data.TotalCount;
                TotalPages = response.Data.TotalPages;
            }
            else
            {
                MessageBox.Show(response.Message ?? "Mahsulotlarni yuklashda xatolik!", "Xatolik",
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
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await _productService.GetCategoriesAsync();

            if (response.Success && response.Data != null)
            {
                Categories.Clear();
                foreach (var category in response.Data)
                {
                    Categories.Add(category);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kategoriyalarni yuklashda xatolik: {ex.Message}", "Xatolik",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private void OpenAddDialog()
    {
        ClearForm();
        IsEditMode = false;

        _activeDialog = new Views.AddEditProductDialog(this);
        _activeDialog.ShowDialog();
    }

    [RelayCommand]
    private void OpenEditDialog()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Iltimos, mahsulotni tanlang!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsEditMode = true;
        EditingProductId = SelectedProduct.Id;
        FormName = SelectedProduct.Name;
        FormDescription = SelectedProduct.Description;
        FormQrCode = SelectedProduct.QrCode;
        FormPrice = SelectedProduct.Price;
        FormStockQuantity = SelectedProduct.StockQuantity;
        FormCategoryId = SelectedProduct.CategoryId;
        FormImageUrl = SelectedProduct.ImageUrl;
        FormMinStockLevel = SelectedProduct.MinStockLevel;
        FormWeight = SelectedProduct.Weight ?? string.Empty;
        FormDimensions = SelectedProduct.Dimensions ?? string.Empty;
        FormManufacturer = SelectedProduct.Manufacturer ?? string.Empty;

        _activeDialog = new Views.AddEditProductDialog(this);
        _activeDialog.ShowDialog();
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(FormName) || FormPrice <= 0 || FormCategoryId <= 0)
        {
            MessageBox.Show("Iltimos, barcha majburiy maydonlarni to'ldiring!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            if (IsEditMode)
            {
                var updateDto = new UpdateProductDto
                {
                    Id = EditingProductId,
                    Name = FormName,
                    Description = FormDescription,
                    Price = FormPrice,
                    StockQuantity = FormStockQuantity,
                    CategoryId = FormCategoryId,
                    ImageUrl = FormImageUrl,
                    MinStockLevel = FormMinStockLevel,
                    Weight = FormWeight,
                    Dimensions = FormDimensions,
                    Manufacturer = FormManufacturer
                };

                var response = await _productService.UpdateProductAsync(updateDto);

                if (response.Success)
                {
                    MessageBox.Show("Mahsulot muvaffaqiyatli yangilandi!", "Muvaffaqiyat",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadProductsAsync();
                    ClearForm();
                    CloseDialog();
                }
                else
                {
                    MessageBox.Show(response.Message ?? "Mahsulotni yangilashda xatolik!", "Xatolik",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                var createDto = new CreateProductDto
                {
                    Name = FormName,
                    Description = FormDescription,
                    QrCode = FormQrCode,
                    Price = FormPrice,
                    StockQuantity = FormStockQuantity,
                    CategoryId = FormCategoryId,
                    ImageUrl = FormImageUrl,
                    MinStockLevel = FormMinStockLevel,
                    Weight = FormWeight,
                    Dimensions = FormDimensions,
                    Manufacturer = FormManufacturer
                };

                var response = await _productService.CreateProductAsync(createDto);

                if (response.Success)
                {
                    MessageBox.Show("Mahsulot muvaffaqiyatli qo'shildi!", "Muvaffaqiyat",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadProductsAsync();
                    ClearForm();
                    CloseDialog();
                }
                else
                {
                    MessageBox.Show(response.Message ?? "Mahsulotni qo'shishda xatolik!", "Xatolik",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Iltimos, mahsulotni tanlang!", "Ogohlantirish",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"{SelectedProduct.Name} mahsulotini o'chirmoqchimisiz?",
            "Tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            try
            {
                var response = await _productService.DeleteProductAsync(SelectedProduct.Id);

                if (response.Success)
                {
                    MessageBox.Show("Mahsulot muvaffaqiyatli o'chirildi!", "Muvaffaqiyat",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadProductsAsync();
                }
                else
                {
                    MessageBox.Show(response.Message ?? "Mahsulotni o'chirishda xatolik!", "Xatolik",
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
        FormName = string.Empty;
        FormDescription = string.Empty;
        FormQrCode = string.Empty;
        FormPrice = 0;
        FormStockQuantity = 0;
        FormCategoryId = 0;
        FormImageUrl = string.Empty;
        FormMinStockLevel = 10;
        FormWeight = string.Empty;
        FormDimensions = string.Empty;
        FormManufacturer = string.Empty;
    }

    private void CloseDialog()
    {
        _activeDialog?.Close();
        _activeDialog = null;
    }
}
