using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Order;
using ECommerce.Huit.Application.Validators.Order;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Huit.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;

    public OrdersController(
        IOrderService orderService,
        IValidator<CreateOrderRequest> createOrderValidator)
    {
        _orderService = orderService;
        _createOrderValidator = createOrderValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var validation = await _createOrderValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(new { error = "ValidationFailed", details = validation.Errors });

        var userId = GetCurrentUserId();
        try
        {
            var order = await _orderService.CreateOrderAsync(userId, request);
            return CreatedAtAction(nameof(GetOrderByCode), new { code = order.Code }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var orders = await _orderService.GetOrdersByUserIdAsync(userId, page, pageSize);
        return Ok(orders);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetOrderByCode(string code)
    {
        var order = await _orderService.GetOrderByCodeAsync(code);
        if (order == null)
            return NotFound(new { error = "OrderNotFound" });

        return Ok(order);
    }

    [HttpPost("{orderId:int}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId, [FromBody] CancelOrderRequest request)
    {
        var success = await _orderService.CancelOrderAsync(orderId, request.Reason);
        if (!success)
            return BadRequest(new { error = "CannotCancelOrder" });

        return NoContent();
    }

    [HttpPost("{orderId:int}/confirm")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> ConfirmOrder(int orderId)
    {
        var staffId = GetCurrentUserId();
        var success = await _orderService.ConfirmOrderAsync(orderId, staffId);
        if (!success)
            return BadRequest(new { error = "CannotConfirmOrder" });

        return NoContent();
    }

    [HttpPost("{orderId:int}/ship")]
    [Authorize(Roles = "ADMIN,STAFF,WAREHOUSE")]
    public async Task<IActionResult> ShipOrder(int orderId, [FromBody] ShipOrderRequest request)
    {
        var success = await _orderService.ShipOrderAsync(orderId, request.WarehouseId, request.SerialNumbersJson);
        if (!success)
            return BadRequest(new { error = "CannotShipOrder" });

        return NoContent();
    }

    [HttpPost("{orderId:int}/complete")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
        var success = await _orderService.CompleteOrderAsync(orderId);
        if (!success)
            return BadRequest(new { error = "CannotCompleteOrder" });

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");
        if (userIdClaim == null)
            throw new UnauthorizedAccessException();

        return int.Parse(userIdClaim.Value);
    }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ShipOrderRequest
{
    public int WarehouseId { get; set; }
    public string SerialNumbersJson { get; set; } = string.Empty; // ["IMEI001","IMEI002"]
}
