using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.Application.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        // TODO: Implement with transaction and stored procedure call
        throw new NotImplementedException();
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
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return false;

        if (order.Status != OrderStatus.PENDING && order.Status != OrderStatus.CONFIRMED && order.Status != OrderStatus.PROCESSING)
            return false;

        // TODO: Implement with transaction and inventory rollback
        order.Status = OrderStatus.CANCELLED;
        order.Note = reason;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmOrderAsync(int orderId, int? staffId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.PENDING) return false;

        order.Status = OrderStatus.CONFIRMED;
        order.UpdatedAt = DateTime.UtcNow;

        // Add status history
        var history = new OrderStatusHistory
        {
            OrderId = orderId,
            Status = OrderStatus.CONFIRMED.ToString(),
            ChangedBy = staffId,
            Note = "Đơn hàng đã được xác nhận"
        };
        _context.OrderStatusHistories.Add(history);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ShipOrderAsync(int orderId, int warehouseId, string serialNumbersJson)
    {
        // TODO: Implement proper serial allocation and inventory updates
        throw new NotImplementedException();
    }

    public async Task<bool> CompleteOrderAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null || order.Status != OrderStatus.SHIPPING) return false;

        order.Status = OrderStatus.COMPLETED;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
