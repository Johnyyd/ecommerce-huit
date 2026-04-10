using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Order;
using HuitShopDB.Models;

namespace HuitShopDB.Services
{
    public class OrderService : IOrderService
    {
        private readonly HuitShopDBDataContext _context;

        public OrderService()
        {
            _context = new HuitShopDBDataContext();
        }

        public async Task<OrderResponseDto> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            // Get cart with items and variant/product
            var cart = _context.carts
                .FirstOrDefault(c => c.user_id == userId);

            if (cart == null || cart.cart_items == null || !cart.cart_items.Any())
                throw new InvalidOperationException("Giỏ hàng trống");

            // Compute subtotal
            decimal subtotal = 0;
            foreach (var ci in cart.cart_items)
            {
                subtotal += ci.quantity * (ci.product_variant != null ? ci.product_variant.price : 0m);
            }

            // Voucher discount calculation
            decimal discount = 0;
            int? voucherId = null;
            if (!string.IsNullOrEmpty(cart.voucher_code))
            {
                var voucher = _context.vouchers
                    .FirstOrDefault(v => v.code == cart.voucher_code
                        && v.is_active == true
                        && v.start_date <= DateTime.UtcNow
                        && v.end_date >= DateTime.UtcNow
                        && (v.usage_limit == null || v.usage_count < v.usage_limit)
                        && subtotal >= v.min_order_value);

                if (voucher == null)
                    throw new InvalidOperationException("Voucher không hợp lệ hoặc đã hết hạn");

                if (voucher.discount_type == "PERCENT")
                {
                    discount = subtotal * (voucher.discount_value / 100);
                    if (voucher.max_discount_amount.HasValue && discount > voucher.max_discount_amount.Value)
                        discount = voucher.max_discount_amount.Value;
                }
                else if (voucher.discount_type == "FIXED")
                {
                    discount = voucher.discount_value;
                }

                if (discount > subtotal) discount = subtotal;
                voucherId = voucher.id;
            }

            // Shipping and tax (free and no tax for now)
            decimal shippingFee = 0;
            decimal taxAmount = 0;
            decimal total = subtotal - discount + shippingFee + taxAmount;

            // Generate unique order code
            string orderCode = "ORD" + Guid.NewGuid().ToString("N").Substring(0, 14);

            // Create order
            var order = new order();
            order.user_id = userId;
            order.code = orderCode;
            order.subtotal = subtotal;
            order.discount = discount;
            order.shipping_fee = shippingFee;
            order.tax_amount = taxAmount;
            order.total = total;
            order.payment_method = request.PaymentMethod;
            order.shipping_address = request.ShippingAddressJson;
            order.status = "PENDING";
            order.payment_status = request.PaymentMethod == "COD" ? "PENDING" : "PAID";
            order.order_type = "ONLINE";
            order.created_at = DateTime.UtcNow;

            // Create order items
            var orderItems = new List<order_item>();
            foreach (var ci in cart.cart_items)
            {
                var item = new order_item();
                item.order = order;
                item.variant_id = ci.variant_id;
                item.product_name = ci.product_variant.product.name + (string.IsNullOrEmpty(ci.product_variant.variant_name) ? "" : " " + ci.product_variant.variant_name);
                item.sku = ci.product_variant.sku;
                item.quantity = ci.quantity;
                item.unit_price = ci.product_variant.price;
                item.total_price = ci.quantity * ci.product_variant.price;
                item.discount_amount = 0m;
                item.created_at = DateTime.UtcNow;
                orderItems.Add(item);
            }

            // Reserve inventory
            int defaultWarehouseId = 1;
            foreach (var ci in cart.cart_items)
            {
                var inventory = _context.inventories
                    .FirstOrDefault(i => i.warehouse_id == defaultWarehouseId && i.variant_id == ci.variant_id);
                if (inventory == null)
                    throw new InvalidOperationException(string.Format("Không tìm thấy tồn kho cho sản phẩm {0}", ci.variant_id));

                int available = inventory.quantity_on_hand - inventory.quantity_reserved;
                if (available < ci.quantity)
                    throw new InvalidOperationException(string.Format("Không đủ tồn kho cho sản phẩm {0}", ci.variant_id));

                inventory.quantity_reserved += ci.quantity;

                var sm = new stock_movement();
                sm.warehouse_id = defaultWarehouseId;
                sm.variant_id = ci.variant_id;
                sm.quantity = -ci.quantity;
                sm.movement_type = "SALE_RESERVED";
                sm.reference_type = "ORDER";
                sm.note = string.Format("Reserve for order {0}", orderCode);
                sm.created_at = DateTime.UtcNow;
                _context.stock_movements.InsertOnSubmit(sm);
            }

            // Voucher usage
            if (voucherId.HasValue)
            {
                var usage = new voucher_usage();
                usage.voucher_id = voucherId.Value;
                usage.user_id = userId;
                usage.order = order;
                usage.discount_amount = discount;
                _context.voucher_usages.InsertOnSubmit(usage);

                var voucher = _context.vouchers.FirstOrDefault(v => v.id == voucherId.Value);
                if (voucher != null)
                {
                    voucher.usage_count++;
                }
            }

            // Order status history
            var history = new order_status_history();
            history.order = order;
            history.status = "PENDING";
            history.note = "Đơn hàng được tạo";
            history.created_at = DateTime.UtcNow;
            _context.order_status_histories.InsertOnSubmit(history);

            // Add order
            _context.orders.InsertOnSubmit(order);
            foreach(var item in orderItems) {
                _context.order_items.InsertOnSubmit(item);
            }

            // Clear cart items
            var cartItemsToRemove = cart.cart_items.ToList();
            _context.cart_items.DeleteAllOnSubmit(cartItemsToRemove);

            // Save all changes
            _context.SubmitChanges();

            return await GetOrderByCodeAsync(orderCode);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 20)
        {
            var orders = _context.orders
                .Where(o => o.user_id == userId)
                .OrderByDescending(o => o.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<OrderResponseDto>();
            foreach (var o in orders)
            {
                result.Add(MapToOrderResponseDto(o));
            }
            return await Task.FromResult(result);
        }

        public async Task<OrderResponseDto> GetOrderByCodeAsync(string orderCode)
        {
            var order = _context.orders
                .FirstOrDefault(o => o.code == orderCode);

            if (order == null) return null;

            return await Task.FromResult(MapToOrderResponseDto(order));
        }

        private OrderResponseDto MapToOrderResponseDto(order order)
        {
            var dto = new OrderResponseDto();
            dto.Id = order.id;
            dto.Code = order.code;
            dto.Subtotal = order.subtotal;
            dto.Discount = order.discount;
            dto.ShippingFee = order.shipping_fee;
            dto.Total = order.total;
            dto.PaymentMethod = order.payment_method;
            dto.PaymentStatus = order.payment_status;
            dto.Status = order.status;
            dto.ShippingAddressJson = order.shipping_address;
            dto.Note = order.note;
            dto.CreatedAt = order.created_at;

            dto.Items = new List<OrderItemDto>();
            foreach (var oi in order.order_items)
            {
                var itemDto = new OrderItemDto();
                itemDto.Id = oi.id;
                itemDto.ProductName = oi.product_name;
                itemDto.Sku = oi.sku;
                itemDto.Quantity = oi.quantity;
                itemDto.UnitPrice = oi.unit_price;
                itemDto.TotalPrice = oi.total_price;
                itemDto.SerialNumbers = new List<string>();
                if (oi.order_item_serials != null)
                {
                    foreach (var s in oi.order_item_serials)
                    {
                        itemDto.SerialNumbers.Add(s.serial_number);
                    }
                }
                dto.Items.Add(itemDto);
            }

            dto.StatusHistory = new List<OrderStatusHistoryDto>();
            if (order.order_status_histories != null)
            {
                foreach (var sh in order.order_status_histories.OrderBy(h => h.created_at))
                {
                    var shDto = new OrderStatusHistoryDto();
                    shDto.Id = sh.id;
                    shDto.Status = sh.status;
                    shDto.Note = sh.note;
                    shDto.CreatedAt = sh.created_at;
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
