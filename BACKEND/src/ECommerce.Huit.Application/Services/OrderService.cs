using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Order;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.Application.Services;

public class OrderService : IOrderService
{
    private readonly IApplicationDbContext _context;

    public OrderService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        // Extract cart items for the user
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Variant)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Giỏ hàng trống");

        // Build JSON array for order items
        var orderItemsJson = System.Text.Json.JsonSerializer.Serialize(
            cart.Items.Select(ci => new
            {
                variant_id = ci.VariantId,
                quantity = ci.Quantity,
                price_at_time = ci.Variant.Price
            })
        );

        // Build voucher code if cart has one
        var voucherCode = cart.VoucherCode;

        // Call stored procedure
        var orderCodeParam = new Microsoft.Data.SqlClient.SqlParameter("@OrderCode", System.Data.SqlDbType.VarChar, 20)
        {
            Direction = System.Data.ParameterDirection.Output
        };
        var orderIdParam = new Microsoft.Data.SqlClient.SqlParameter("@OrderID", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };

        var parameters = new[]
        {
            new Microsoft.Data.SqlClient.SqlParameter("@UserID", userId),
            new Microsoft.Data.SqlClient.SqlParameter("@ShippingAddress", request.ShippingAddressJson ?? (object)DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@PaymentMethod", request.PaymentMethod ?? (object)DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@VoucherCode", string.IsNullOrEmpty(voucherCode) ? (object)DBNull.Value : voucherCode),
            new Microsoft.Data.SqlClient.SqlParameter("@OrderItemsJSON", orderItemsJson),
            orderIdParam,
            orderCodeParam
        };

        await _context.ExecuteSqlRawAsync(
            "EXEC sp_CreateOrder @UserID, @ShippingAddress, @PaymentMethod, @VoucherCode, @OrderItemsJSON, @OrderID OUTPUT, @OrderCode OUTPUT",
            parameters
        );

        var orderId = (int)orderIdParam.Value;
        var orderCode = (string)orderCodeParam.Value;

        // Return the created order
        return await GetOrderByCodeAsync(orderCode) ?? new OrderResponseDto
        {
            Id = orderId,
            Code = orderCode,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 20)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderResponseDto
            {
                Id = o.Id,
                Code = o.Code,
                Subtotal = o.Subtotal,
                Discount = o.Discount,
                ShippingFee = o.ShippingFee,
                Total = o.Total,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus.ToString(),
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductName = oi.ProductName,
                    Sku = oi.Sku,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    SerialNumbers = oi.OrderItemSerials.Select(ois => ois.SerialNumber).ToList()
                }).ToList()
            })
            .ToListAsync();

        return orders;
    }

    public async Task<OrderResponseDto?> GetOrderByCodeAsync(string orderCode)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(oi => oi.OrderItemSerials)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Code == orderCode);

        if (order == null) return null;

        return new OrderResponseDto
        {
            Id = order.Id,
            Code = order.Code,
            Subtotal = order.Subtotal,
            Discount = order.Discount,
            ShippingFee = order.ShippingFee,
            Total = order.Total,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus.ToString(),
            Status = order.Status.ToString(),
            ShippingAddressJson = order.ShippingAddress,
            Note = order.Note,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductName = oi.ProductName,
                Sku = oi.Sku,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice,
                SerialNumbers = oi.OrderItemSerials.Select(ois => ois.SerialNumber).ToList()
            }).ToList(),
            StatusHistory = order.StatusHistories.Select(sh => new OrderStatusHistoryDto
            {
                Id = sh.Id,
                Status = sh.Status,
                Note = sh.Note,
                CreatedAt = sh.CreatedAt
            }).OrderBy(sh => sh.CreatedAt).ToList()
        };
    }

    public async Task<bool> CancelOrderAsync(int orderId, string reason)
    {
        var parameters = new[]
        {
            new Microsoft.Data.SqlClient.SqlParameter("@OrderID", orderId),
            new Microsoft.Data.SqlClient.SqlParameter("@Reason", reason ?? (object)DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@UserID", (object)DBNull.Value)
        };

        try
        {
            await _context.ExecuteSqlRawAsync(
                "EXEC sp_CancelOrder @OrderID, @Reason, @UserID",
                parameters
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ConfirmOrderAsync(int orderId, int? staffId)
    {
        var parameters = new[]
        {
            new Microsoft.Data.SqlClient.SqlParameter("@OrderID", orderId)
        };

        try
        {
            await _context.ExecuteSqlRawAsync(
                "EXEC sp_ConfirmOrder @OrderID",
                parameters
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ShipOrderAsync(int orderId, int warehouseId, string serialNumbersJson)
    {
        // For now, we'll call sp_ShipOrder without tracking/shipping provider
        // In production, you'd probably pass these as parameters
        var parameters = new[]
        {
            new Microsoft.Data.SqlClient.SqlParameter("@OrderID", orderId),
            new Microsoft.Data.SqlClient.SqlParameter("@TrackingNumber", string.IsNullOrEmpty(serialNumbersJson) ? (object)DBNull.Value : serialNumbersJson),
            new Microsoft.Data.SqlClient.SqlParameter("@ShippingProvider", (object)DBNull.Value),
            new Microsoft.Data.SqlClient.SqlParameter("@UserID", (object)DBNull.Value)
        };

        try
        {
            await _context.ExecuteSqlRawAsync(
                "EXEC sp_ShipOrder @OrderID, @TrackingNumber, @ShippingProvider, @UserID",
                parameters
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CompleteOrderAsync(int orderId)
    {
        var parameters = new[]
        {
            new Microsoft.Data.SqlClient.SqlParameter("@OrderID", orderId),
            new Microsoft.Data.SqlClient.SqlParameter("@UserID", (object)DBNull.Value)
        };

        try
        {
            await _context.ExecuteSqlRawAsync(
                "EXEC sp_CompleteOrder @OrderID, @UserID",
                parameters
            );
            return true;
        }
        catch
        {
            return false;
        }
    }
}
