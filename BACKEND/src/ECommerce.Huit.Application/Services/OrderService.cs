using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Order;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Transactions;

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
        // Get cart with items and variant/product
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
            throw new InvalidOperationException("Giỏ hàng trống");

        // Compute subtotal
        decimal subtotal = cart.Items.Sum(ci => ci.Quantity * ci.Variant.Price);

        // Voucher discount calculation
        decimal discount = 0;
        int? voucherId = null;
        if (!string.IsNullOrEmpty(cart.VoucherCode))
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == cart.VoucherCode
                    && v.IsActive
                    && v.StartDate <= DateTime.UtcNow
                    && v.EndDate >= DateTime.UtcNow
                    && (v.UsageLimit == null || v.UsageCount < v.UsageLimit)
                    && subtotal >= v.MinOrderValue);

            if (voucher == null)
                throw new InvalidOperationException("Voucher không hợp lệ hoặc đã hết hạn");

            if (voucher.DiscountType == DiscountType.PERCENT)
            {
                discount = subtotal * (voucher.DiscountValue / 100);
                if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount)
                    discount = voucher.MaxDiscountAmount.Value;
            }
            else if (voucher.DiscountType == DiscountType.FIXED)
            {
                discount = voucher.DiscountValue;
            }

            if (discount > subtotal) discount = subtotal;
            voucherId = voucher.Id;
        }

        // Shipping and tax (free and no tax for now)
        decimal shippingFee = 0;
        decimal taxAmount = 0;
        decimal total = subtotal - discount + shippingFee + taxAmount;

        // Generate unique order code
        string orderCode = "ORD" + Guid.NewGuid().ToString("N").Substring(0, 14);

        // Create order
        var order = new Order
        {
            UserId = userId,
            Code = orderCode,
            Subtotal = subtotal,
            Discount = discount,
            ShippingFee = shippingFee,
            TaxAmount = taxAmount,
            Total = total,
            PaymentMethod = request.PaymentMethod,
            ShippingAddress = request.ShippingAddressJson,
            Status = OrderStatus.PENDING,
            PaymentStatus = request.PaymentMethod == "COD" ? PaymentStatus.PENDING : PaymentStatus.PAID,
            OrderType = OrderType.ONLINE,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // Create order items
        var orderItems = new List<OrderItem>();
        foreach (var ci in cart.Items)
        {
            var item = new OrderItem
            {
                Order = order,
                VariantId = ci.VariantId,
                ProductName = ci.Variant.Product.Name + (string.IsNullOrEmpty(ci.Variant.VariantName) ? "" : " " + ci.Variant.VariantName),
                Sku = ci.Variant.Sku,
                Quantity = ci.Quantity,
                UnitPrice = ci.Variant.Price,
                TotalPrice = ci.Quantity * ci.Variant.Price,
                DiscountAmount = 0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
            orderItems.Add(item);
        }
        order.Items = orderItems;

        // Reserve inventory (check availability and update)
        // For simplicity, use default warehouse ID = 1 (assumes one warehouse)
        int defaultWarehouseId = 1;
        foreach (var ci in cart.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.WarehouseId == defaultWarehouseId && i.VariantId == ci.VariantId);
            if (inventory == null)
                throw new InvalidOperationException($"Không tìm thấy tồn kho cho sản phẩm {ci.VariantId}");

            int available = inventory.QuantityOnHand - inventory.QuantityReserved;
            if (available < ci.Quantity)
                throw new InvalidOperationException($"Không đủ tồn kho cho sản phẩm {ci.VariantId}");

            inventory.QuantityReserved += ci.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
        }

        // Create stock movements for reservation
        foreach (var ci in cart.Items)
        {
            var sm = new StockMovement
            {
                WarehouseId = defaultWarehouseId,
                VariantId = ci.VariantId,
                Quantity = -ci.Quantity,
                MovementType = MovementType.SALE_RESERVED,
                ReferenceId = null, // will set after order saved
                ReferenceType = "ORDER",
                Note = $"Reserve for order {orderCode}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
            _context.StockMovements.Add(sm);
        }

        // Voucher usage and increment
        if (voucherId.HasValue)
        {
            var usage = new VoucherUsage
            {
                VoucherId = voucherId.Value,
                UserId = userId,
                Order = order,
                DiscountAmount = discount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };
            _context.VoucherUsages.Add(usage);

            var voucher = await _context.Vouchers.FindAsync(voucherId.Value);
            if (voucher != null)
            {
                voucher.UsageCount++;
            }
        }

        // Order status history
        var history = new OrderStatusHistory
        {
            Order = order,
            Status = OrderStatus.PENDING.ToString(),
            Note = "Đơn hàng được tạo",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
        _context.OrderStatusHistories.Add(history);

        // Add order (cascades order items)
        _context.Orders.Add(order);

        // Clear cart items
        cart.Items.Clear();

        // Save all changes
        await _context.SaveChangesAsync();

        // After save, we could update stock movements reference if needed:
        // (Optional) set ReferenceId = order.Id and save again if required.
        // For now leave NULL.

        // Return the order DTO by fetching fresh
        return await GetOrderByCodeAsync(orderCode) ?? new OrderResponseDto
        {
            Id = order.Id,
            Code = order.Code,
            CreatedAt = order.CreatedAt
        };
    }

    // existing GetOrdersByUserIdAsync and GetOrderByCodeAsync remain unchanged...
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

    // Stub implementations for other actions (will be implemented later)
    public Task<bool> CancelOrderAsync(int orderId, string reason)
    {
        // TODO: Implement cancellation logic with inventory return, etc.
        throw new NotImplementedException("CancelOrderAsync not implemented yet");
    }

    public Task<bool> ConfirmOrderAsync(int orderId, int? staffId)
    {
        // TODO: Implement confirmation
        throw new NotImplementedException("ConfirmOrderAsync not implemented yet");
    }

    public Task<bool> ShipOrderAsync(int orderId, int warehouseId, string serialNumbersJson)
    {
        // TODO: Implement shipping logic
        throw new NotImplementedException("ShipOrderAsync not implemented yet");
    }

    public Task<bool> CompleteOrderAsync(int orderId)
    {
        // TODO: Implement completion logic
        throw new NotImplementedException("CompleteOrderAsync not implemented yet");
    }
}