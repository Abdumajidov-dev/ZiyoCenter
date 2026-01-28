using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Cart;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private IActionResult? CheckCustomerAccess()
    {
        var userType = GetCurrentUserType();
        if (userType != "Customer")
            return StatusCode(403, new { status = false, message = "Only customers can access cart", data = (object?)null });
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetCartItems()
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.GetCartItemsAsync(customerId);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto request)
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.AddToCartAsync(request, customerId);
        return HandleResult(result);
    }

    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto request)
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.UpdateCartItemAsync(cartItemId, request, customerId);
        return HandleResult(result);
    }

    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.RemoveFromCartAsync(cartItemId, customerId);

        if (!result.IsSuccess)
            return ErrorResponse(result.Message);

        return SuccessResponse(result.Message);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.ClearCartAsync(customerId);

        if (!result.IsSuccess)
            return ErrorResponse(result.Message);

        return SuccessResponse(result.Message);
    }

    [HttpGet("total")]
    public async Task<IActionResult> GetCartTotal()
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.GetCartTotalAsync(customerId);
        return HandleResult(result);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCartItemsCount()
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.GetCartItemsCountAsync(customerId);
        return HandleResult(result);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCart()
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.ValidateCartForCheckoutAsync(customerId);

        if (!result.IsSuccess)
            return ErrorResponse(result.Message);

        return SuccessResponse(result.Message);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedMockCartItems([FromQuery] int count = 10)
    {
        var accessCheck = CheckCustomerAccess();
        if (accessCheck != null) return accessCheck;

        var customerId = GetCurrentUserId();
        var result = await _cartService.SeedMockCartItemsAsync(customerId, count);
        return HandleResult(result);
    }
}
