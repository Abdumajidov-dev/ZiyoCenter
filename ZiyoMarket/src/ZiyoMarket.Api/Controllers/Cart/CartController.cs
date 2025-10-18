using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZiyoMarket.Service.DTOs.Orders;
using ZiyoMarket.Service.Interfaces;

namespace ZiyoMarket.Api.Controllers.Cart;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CartController : BaseController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCartItems()
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.GetCartItemsAsync(customerId);
        return Ok(new { success = true, data = result.Data });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto request)
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.AddToCartAsync(request, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto request)
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.UpdateCartItemAsync(cartItemId, request, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }

    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.RemoveFromCartAsync(cartItemId, customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.ClearCartAsync(customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpGet("total")]
    public async Task<IActionResult> GetCartTotal()
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.GetCartTotalAsync(customerId);
        return Ok(new { success = true, total = result.Data });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCartItemsCount()
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.GetCartItemsCountAsync(customerId);
        return Ok(new { success = true, count = result.Data });
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCart()
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.ValidateCartForCheckoutAsync(customerId);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedMockCartItems([FromQuery] int count = 10)
    {
        var customerId = GetCurrentUserId();
        var result = await _cartService.SeedMockCartItemsAsync(customerId, count);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return Ok(new { success = true, message = result.Message, data = result.Data });
    }
}
