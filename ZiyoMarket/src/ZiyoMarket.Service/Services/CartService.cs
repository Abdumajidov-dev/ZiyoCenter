using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Products;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CartService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<CartItemDto>>> GetCartItemsAsync(int customerId)
    {
        try
        {
            var cartItems = await _unitOfWork.CartItems
                .SelectAll(c => c.CustomerId == customerId, new[] { "Product", "Product.Category" })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var dtos = _mapper.Map<List<CartItemDto>>(cartItems);
            return Result<List<CartItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<CartItemDto>>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CartItemDto>> AddToCartAsync(AddToCartDto request, int customerId)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null || !product.IsActive)
                return Result<CartItemDto>.BadRequest("Product not available");

            var existing = await _unitOfWork.CartItems
                .SelectAsync(c => c.CustomerId == customerId && c.ProductId == request.ProductId);

            if (existing != null)
            {
                existing.Quantity += request.Quantity;
                existing.UnitPrice = product.Price;
                existing.MarkAsUpdated();
                await _unitOfWork.CartItems.Update(existing, existing.Id);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price,
                    AddedAt = DateTime.UtcNow
                };
                await _unitOfWork.CartItems.InsertAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.CartItems
                .SelectAsync(c => c.CustomerId == customerId && c.ProductId == request.ProductId,
                            new[] { "Product" });

            var dto = _mapper.Map<CartItemDto>(updated);
            return Result<CartItemDto>.Success(dto, "Item added to cart");
        }
        catch (Exception ex)
        {
            return Result<CartItemDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<CartItemDto>> UpdateCartItemAsync(int cartItemId, UpdateCartItemDto request, int customerId)
    {
        try
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
            if (cartItem == null || cartItem.CustomerId != customerId)
                return Result<CartItemDto>.NotFound("Cart item not found");

            cartItem.Quantity = request.Quantity;
            cartItem.MarkAsUpdated();

            await _unitOfWork.CartItems.Update(cartItem, cartItemId);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.CartItems
                .SelectAsync(c => c.Id == cartItemId, new[] { "Product" });

            var dto = _mapper.Map<CartItemDto>(updated);
            return Result<CartItemDto>.Success(dto, "Cart item updated");
        }
        catch (Exception ex)
        {
            return Result<CartItemDto>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> RemoveFromCartAsync(int cartItemId, int customerId)
    {
        try
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
            if (cartItem == null || cartItem.CustomerId != customerId)
                return Result.NotFound("Cart item not found");

            cartItem.Delete();
            await _unitOfWork.CartItems.Update(cartItem, cartItemId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Item removed from cart");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ClearCartAsync(int customerId)
    {
        try
        {
            var cartItems = await _unitOfWork.CartItems
                .SelectAll(c => c.CustomerId == customerId)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                item.Delete();
                await _unitOfWork.CartItems.Update(item, item.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success("Cart cleared");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> GetCartTotalAsync(int customerId)
    {
        try
        {
            var total = await _unitOfWork.CartItems
                .SelectAll(c => c.CustomerId == customerId)
                .SumAsync(c => c.UnitPrice * c.Quantity);

            return Result<decimal>.Success(total);
        }
        catch (Exception ex)
        {
            return Result<decimal>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetCartItemsCountAsync(int customerId)
    {
        try
        {
            var count = await _unitOfWork.CartItems
                .CountAsync(c => c.CustomerId == customerId);

            return Result<int>.Success(count);
        }
        catch (Exception ex)
        {
            return Result<int>.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> ValidateCartForCheckoutAsync(int customerId)
    {
        try
        {
            var cartItems = await _unitOfWork.CartItems
                .SelectAll(c => c.CustomerId == customerId, new[] { "Product" })
                .ToListAsync();

            if (!cartItems.Any())
                return Result.BadRequest("Cart is empty");

            var errors = new List<string>();

            foreach (var item in cartItems)
            {
                if (item.Product == null || !item.Product.IsActive)
                    errors.Add($"Product {item.ProductId} is not available");
                else if (item.Product.StockQuantity < item.Quantity)
                    errors.Add($"Insufficient stock for {item.Product.Name}");
            }

            if (errors.Any())
                return Result.BadRequest(string.Join(", ", errors));

            return Result.Success("Cart is valid");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAllCartItemsAsync(int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _unitOfWork.CartItems.Table;

            if (startDate.HasValue)
                query = query.Where(c => c.AddedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.AddedAt <= endDate.Value);

            var items = await query.ToListAsync();

            foreach (var item in items)
            {
                item.Delete();
                await _unitOfWork.CartItems.Update(item, item.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            return Result.Success($"{items.Count} cart items deleted");
        }
        catch (Exception ex)
        {
            return Result.InternalError($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CartItemDto>>> SeedMockCartItemsAsync(int customerId, int count = 10)
    {
        try
        {
            var random = new Random();
            var products = await _unitOfWork.Products
                .SelectAll(p => p.IsActive)
                .Take(20)
                .ToListAsync();

            if (!products.Any())
                return Result<List<CartItemDto>>.BadRequest("No products available");

            var cartItems = new List<CartItem>();

            for (int i = 0; i < count; i++)
            {
                var product = products[random.Next(products.Count)];
                var cartItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = product.Id,
                    Quantity = random.Next(1, 5),
                    UnitPrice = product.Price,
                    AddedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                };

                await _unitOfWork.CartItems.InsertAsync(cartItem);
                cartItems.Add(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();

            var dtos = _mapper.Map<List<CartItemDto>>(cartItems);
            return Result<List<CartItemDto>>.Success(dtos, $"{count} mock cart items created");
        }
        catch (Exception ex)
        {
            return Result<List<CartItemDto>>.InternalError($"Error: {ex.Message}");
        }
    }
}

