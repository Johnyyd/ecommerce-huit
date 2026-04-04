using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Order;
using ECommerce.Huit.Domain.Entities;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Application.Services
{
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
                .Include(c => c.Items.Select(ci => ci.Variant.Product))
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                throw new InvalidOperationException("Giỏ hàng trống");

            // Compute subtotal
            decimal subtotal = 0;
            foreach (var ci in cart.Items)
            {
                subtotal += ci.Quantity * ci.Variant.Price;
            }

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
                    if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
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
            var order = new Order();
            order.UserId = userId;
            order.Code = orderCode;
            order.Subtotal = subtotal;
            order.Discount = discount;
            order.ShippingFee = shippingFee;
            order.TaxAmount = taxAmount;
            order.Total = total;
            order.PaymentMethod = request.PaymentMethod;
            order.ShippingAddress = request.ShippingAddressJson;
            order.Status = OrderStatus.PENDING;
            order.PaymentStatus = request.PaymentMethod == "COD" ? PaymentStatus.PENDING : PaymentStatus.PAID;
            order.OrderType = OrderType.ONLINE;
            order.CreatedAt = DateTime.UtcNow;

            // Create order items
            var orderItems = new List<OrderItem>();
            foreach (var ci in cart.Items)
            {
                var item = new OrderItem();
                item.Order = order;
                item.VariantId = ci.VariantId;
                item.ProductName = ci.Variant.Product.Name + (string.IsNullOrEmpty(ci.Variant.VariantName) ? "" : " " + ci.Variant.VariantName);
                item.Sku = ci.Variant.Sku;
                item.Quantity = ci.Quantity;
                item.UnitPrice = ci.Variant.Price;
                item.TotalPrice = ci.Quantity * ci.Variant.Price;
                item.DiscountAmount = 0m;
                item.CreatedAt = DateTime.UtcNow;
                orderItems.Add(item);
            }
            order.Items = orderItems;

            // Reserve inventory
            int defaultWarehouseId = 1;
            foreach (var ci in cart.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.WarehouseId == defaultWarehouseId && i.VariantId == ci.VariantId);
                if (inventory == null)
                    throw new InvalidOperationException(string.Format("Không tìm thấy tồn kho cho sản phẩm {0}", ci.VariantId));

                int available = inventory.QuantityOnHand - inventory.QuantityReserved;
                if (available < ci.Quantity)
                    throw new InvalidOperationException(string.Format("Không đủ tồn kho cho sản phẩm {0}", ci.VariantId));

                inventory.QuantityReserved += ci.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;

                var sm = new StockMovement();
                sm.WarehouseId = defaultWarehouseId;
                sm.VariantId = ci.VariantId;
                sm.Quantity = -ci.Quantity;
                sm.MovementType = MovementType.SALE_RESERVED;
                sm.ReferenceType = "ORDER";
                sm.Note = string.Format("Reserve for order {0}", orderCode);
                sm.CreatedAt = DateTime.UtcNow;
                _context.StockMovements.Add(sm);
            }

            // Voucher usage
            if (voucherId.HasValue)
            {
                var usage = new VoucherUsage();
                usage.VoucherId = voucherId.Value;
                usage.UserId = userId;
                usage.Order = order;
                usage.DiscountAmount = discount;
                usage.CreatedAt = DateTime.UtcNow;
                _context.VoucherUsages.Add(usage);

                var voucher = await _context.Vouchers.FindAsync(voucherId.Value);
                if (voucher != null)
                {
                    voucher.UsageCount++;
                }
            }

            // Order status history
            var history = new OrderStatusHistory();
            history.Order = order;
            history.Status = OrderStatus.PENDING.ToString();
            history.Note = "Đơn hàng được tạo";
            history.CreatedAt = DateTime.UtcNow;
            _context.OrderStatusHistories.Add(history);

            // Add order
            _context.Orders.Add(order);

            // Clear cart items
            var cartItemsToRemove = cart.Items.ToList();
            foreach (var item in cartItemsToRemove)
            {
                _context.CartItems.Remove(item);
            }

            // Save all changes
            await _context.SaveChangesAsync();

            return await GetOrderByCodeAsync(orderCode);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 20)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<OrderResponseDto>();
            foreach (var o in orders)
            {
                result.Add(MapToOrderResponseDto(o));
            }
            return result;
        }

        public async Task<OrderResponseDto> GetOrderByCodeAsync(string orderCode)
        {
            var order = await _context.Orders
                .Include(o => o.Items.Select(oi => oi.OrderItemSerials))
                .Include(o => o.StatusHistories)
                .FirstOrDefaultAsync(o => o.Code == orderCode);

            if (order == null) return null;

            return MapToOrderResponseDto(order);
        }

        private OrderResponseDto MapToOrderResponseDto(Order order)
        {
            var dto = new OrderResponseDto();
            dto.Id = order.Id;
            dto.Code = order.Code;
            dto.Subtotal = order.Subtotal;
            dto.Discount = order.Discount;
            dto.ShippingFee = order.ShippingFee;
            dto.Total = order.Total;
            dto.PaymentMethod = order.PaymentMethod;
            dto.PaymentStatus = order.PaymentStatus.ToString();
            dto.Status = order.Status.ToString();
            dto.ShippingAddressJson = order.ShippingAddress;
            dto.Note = order.Note;
            dto.CreatedAt = order.CreatedAt;

            dto.Items = new List<OrderItemDto>();
            foreach (var oi in order.Items)
            {
                var itemDto = new OrderItemDto();
                itemDto.Id = oi.Id;
                itemDto.ProductName = oi.ProductName;
                itemDto.Sku = oi.Sku;
                itemDto.Quantity = oi.Quantity;
                itemDto.UnitPrice = oi.UnitPrice;
                itemDto.TotalPrice = oi.TotalPrice;
                itemDto.SerialNumbers = new List<string>();
                if (oi.OrderItemSerials != null)
                {
                    foreach (var s in oi.OrderItemSerials)
                    {
                        itemDto.SerialNumbers.Add(s.SerialNumber);
                    }
                }
                dto.Items.Add(itemDto);
            }

            dto.StatusHistory = new List<OrderStatusHistoryDto>();
            if (order.StatusHistories != null)
            {
                foreach (var sh in order.StatusHistories.OrderBy(h => h.CreatedAt))
                {
                    var shDto = new OrderStatusHistoryDto();
                    shDto.Id = sh.Id;
                    shDto.Status = sh.Status;
                    shDto.Note = sh.Note;
                    shDto.CreatedAt = sh.CreatedAt;
                    dto.StatusHistory.Add(shDto);
                }
            }

            return dto;
        }

        public Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConfirmOrderAsync(int orderId, int? staffId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ShipOrderAsync(int orderId, int warehouseId, string serialNumbersJson)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CompleteOrderAsync(int orderId)
        {
            throw new NotImplementedException();
        }
    }
}