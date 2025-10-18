using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Interfaces;

public interface ICartService
{
    Task<Result<List<CartItemDto>>> GetCartItemsAsync(int customerId);
    Task<Result<CartItemDto>> AddToCartAsync(AddToCartDto request, int customerId);
    Task<Result<CartItemDto>> UpdateCartItemAsync(int cartItemId, UpdateCartItemDto request, int customerId);
    Task<Result> RemoveFromCartAsync(int cartItemId, int customerId);
    Task<Result> ClearCartAsync(int customerId);
    Task<Result<decimal>> GetCartTotalAsync(int customerId);
    Task<Result<int>> GetCartItemsCountAsync(int customerId);
    Task<Result> ValidateCartForCheckoutAsync(int customerId);

    // Bulk operations
    Task<Result> DeleteAllCartItemsAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null);
    Task<Result<List<CartItemDto>>> SeedMockCartItemsAsync(int customerId, int count = 10);
}