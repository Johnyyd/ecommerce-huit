using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Cart;
using ECommerce.Huit.Application.Validators.Cart;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Huit.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly IValidator<AddCartItemRequest> _addItemValidator;
    private readonly IValidator<UpdateCartItemRequest> _updateItemValidator;

    public CartController(
        ICartService cartService,
        IValidator<AddCartItemRequest> addItemValidator,
        IValidator<UpdateCartItemRequest> updateItemValidator)
    {
        _cartService = cartService;
        _addItemValidator = addItemValidator;
        _updateItemValidator = updateItemValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetCurrentUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        var validation = await _addItemValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { error = "ValidationFailed", details = validation.Errors });

        var userId = GetCurrentUserId();
        try
        {
            var cart = await _cartService.AddItemAsync(userId, request);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "ValidationError", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "CartError", message = ex.Message });
        }
    }

    [HttpPut("items/{itemId:int}")]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemRequest request)
    {
        var validation = await _updateItemValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { error = "ValidationFailed", details = validation.Errors });

        var userId = GetCurrentUserId();
        try
        {
            var cart = await _cartService.UpdateItemAsync(userId, itemId, request.Quantity);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "ValidationError", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "CartError", message = ex.Message });
        }
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var userId = GetCurrentUserId();
        var success = await _cartService.RemoveItemAsync(userId, itemId);
        if (!success)
            return NotFound(new { error = "ItemNotFound" });

        return NoContent();
    }

    [HttpPost("apply-voucher")]
    public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request)
    {
        var userId = GetCurrentUserId();
        var cart = await _cartService.ApplyVoucherAsync(userId, request.Code);
        return Ok(cart);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetCurrentUserId();
        var cart = await _cartService.ClearCartAsync(userId);
        return Ok(cart);
    }

    private int GetCurrentUserId()
    {
        // Extract user ID from JWT claim
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");
        if (userIdClaim == null)
            throw new UnauthorizedAccessException();

        return int.Parse(userIdClaim.Value);
    }
}

public class ApplyVoucherRequest
{
    public string Code { get; set; } = string.Empty;
}
