using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Order;
using HuitShopDB.Models;
using Newtonsoft.Json;

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
            var cart = _context.carts.FirstOrDefault(c => c.user_id == userId);

            if (cart == null || cart.cart_items == null || !cart.cart_items.Any())
                throw new InvalidOperationException("Giỏ hàng trống");

            decimal subtotal = 0;
            foreach (var ci in cart.cart_items)
            {
                subtotal += ci.quantity * (ci.product_variant != null ? ci.product_variant.price : 0m);
            }

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

                if (voucher != null)
                {
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
            }

            // Phí vận chuyển: miễn phí từ 500,000đ
            decimal shippingFee = subtotal >= 500000m ? 0m : 30000m;
            decimal taxAmount = 0;
            decimal total = subtotal - discount + shippingFee + taxAmount;

            string orderCode = "ORD" + Guid.NewGuid().ToString("N").Substring(0, 14).ToUpper();

            var orderObj = new order();
            orderObj.user_id = userId;
            orderObj.code = orderCode;
            orderObj.subtotal = subtotal;
            orderObj.discount = discount;
            orderObj.shipping_fee = shippingFee;
            orderObj.tax_amount = taxAmount;
            orderObj.total = total;
            orderObj.payment_method = request.PaymentMethod;
            orderObj.shipping_address = request.ShippingAddressJson;
            orderObj.note = request.Note;
            orderObj.status = "PENDING";
            orderObj.payment_status = request.PaymentMethod == "COD" ? "PENDING" : "PAID";
            orderObj.order_type = "ONLINE";
            orderObj.created_at = DateTime.UtcNow;

            var orderItems = new List<order_item>();
            foreach (var ci in cart.cart_items)
            {
                var item = new order_item();
                item.order = orderObj;
                item.variant_id = ci.variant_id;
                item.product_name = ci.product_variant.product.name +
                    (string.IsNullOrEmpty(ci.product_variant.variant_name) ? "" : " " + ci.product_variant.variant_name);
                item.sku = ci.product_variant.sku;
                item.quantity = ci.quantity;
                item.unit_price = ci.product_variant.price;
                item.total_price = ci.quantity * ci.product_variant.price;
                item.discount_amount = 0m;
                item.created_at = DateTime.UtcNow;
                orderItems.Add(item);
            }

            // Khóa tồn kho (reserve)
            int defaultWarehouseId = 1;
            foreach (var ci in cart.cart_items)
            {
                var inventory = _context.inventories
                    .FirstOrDefault(i => i.warehouse_id == defaultWarehouseId && i.variant_id == ci.variant_id);
                if (inventory == null)
                    throw new InvalidOperationException(string.Format("Không tìm thấy tồn kho cho sản phẩm #{0}", ci.variant_id));

                int available = inventory.quantity_on_hand - inventory.quantity_reserved;
                if (available < ci.quantity)
                    throw new InvalidOperationException(string.Format("Không đủ tồn kho cho sản phẩm #{0}", ci.variant_id));

                inventory.quantity_reserved += ci.quantity;

                var sm = new stock_movement();
                sm.warehouse_id = defaultWarehouseId;
                sm.variant_id = ci.variant_id;
                sm.quantity = -ci.quantity;
                sm.movement_type = "SALE_RESERVED";
                sm.reference_type = "ORDER";
                sm.note = string.Format("Khóa tồn kho cho đơn hàng {0}", orderCode);
                sm.created_at = DateTime.UtcNow;
                _context.stock_movements.InsertOnSubmit(sm);
            }

            // Ghi nhận voucher usage
            if (voucherId.HasValue)
            {
                var usage = new voucher_usage();
                usage.voucher_id = voucherId.Value;
                usage.user_id = userId;
                usage.order = orderObj;
                usage.discount_amount = discount;
                _context.voucher_usages.InsertOnSubmit(usage);

                var voucherEntity = _context.vouchers.FirstOrDefault(v => v.id == voucherId.Value);
                if (voucherEntity != null)
                    voucherEntity.usage_count++;
            }

            // Lịch sử trạng thái đơn hàng
            var history = new order_status_history();
            history.order = orderObj;
            history.status = "PENDING";
            history.note = "Đơn hàng được tạo thành công";
            history.created_at = DateTime.UtcNow;
            _context.order_status_histories.InsertOnSubmit(history);

            _context.orders.InsertOnSubmit(orderObj);
            foreach (var item in orderItems)
            {
                _context.order_items.InsertOnSubmit(item);
            }

            // Xóa giỏ hàng
            var cartItemsToRemove = cart.cart_items.ToList();
            _context.cart_items.DeleteAllOnSubmit(cartItemsToRemove);

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
                result.Add(MapToOrderResponseDto(o));

            return await Task.FromResult(result);
        }

        public async Task<OrderResponseDto> GetOrderByCodeAsync(string orderCode)
        {
            var o = _context.orders.FirstOrDefault(x => x.code == orderCode);
            if (o == null) return null;
            return await Task.FromResult(MapToOrderResponseDto(o));
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(int orderId)
        {
            var o = _context.orders.FirstOrDefault(x => x.id == orderId);
            if (o == null) return null;
            return await Task.FromResult(MapToOrderResponseDto(o));
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync(string status, string keyword, int page, int pageSize)
        {
            var query = _context.orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "ALL")
                query = query.Where(o => o.status == status);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(o => o.code.Contains(keyword));

            var orders = query
                .OrderByDescending(o => o.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<OrderResponseDto>();
            foreach (var o in orders)
                result.Add(MapToOrderResponseDto(o));

            return await Task.FromResult(result);
        }

        public async Task<int> GetAllOrdersCountAsync(string status, string keyword)
        {
            var query = _context.orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "ALL")
                query = query.Where(o => o.status == status);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(o => o.code.Contains(keyword));

            return await Task.FromResult(query.Count());
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason)
        {
            var o = _context.orders.FirstOrDefault(x => x.id == orderId);
            if (o == null || (o.status != "PENDING" && o.status != "CONFIRMED"))
                return await Task.FromResult(false);

            var previousStatus = o.status;
            o.status = "CANCELLED";

            // Hoàn trả tồn kho đã khóa
            foreach (var oi in o.order_items)
            {
                var inventory = _context.inventories.FirstOrDefault(i => i.variant_id == oi.variant_id && i.warehouse_id == 1);
                if (inventory != null)
                    inventory.quantity_reserved -= oi.quantity;

                var sm = new stock_movement();
                sm.warehouse_id = 1;
                sm.variant_id = oi.variant_id;
                sm.quantity = oi.quantity;
                sm.movement_type = "CANCEL_RELEASE";
                sm.reference_type = "ORDER";
                sm.note = string.Format("Hoàn trả kho do hủy đơn {0}", o.code);
                sm.created_at = DateTime.UtcNow;
                _context.stock_movements.InsertOnSubmit(sm);
            }

            var history = new order_status_history();
            history.order_id = orderId;
            history.status = "CANCELLED";
            history.note = string.IsNullOrEmpty(reason) ? "Đơn hàng bị hủy" : reason;
            history.created_at = DateTime.UtcNow;
            _context.order_status_histories.InsertOnSubmit(history);

            _context.SubmitChanges();
            return await Task.FromResult(true);
        }

        public async Task<bool> ConfirmOrderAsync(int orderId, int? staffId)
        {
            var o = _context.orders.FirstOrDefault(x => x.id == orderId);
            if (o == null || o.status != "PENDING")
                return await Task.FromResult(false);

            o.status = "CONFIRMED";

            var history = new order_status_history();
            history.order_id = orderId;
            history.status = "CONFIRMED";
            history.note = "Đơn hàng đã được xác nhận";
            if (staffId.HasValue) history.changed_by = staffId.Value;
            history.created_at = DateTime.UtcNow;
            _context.order_status_histories.InsertOnSubmit(history);

            _context.SubmitChanges();
            return await Task.FromResult(true);
        }

        public async Task<bool> ShipOrderAsync(int orderId, int warehouseId, string serialNumbersJson)
        {
            var o = _context.orders.FirstOrDefault(x => x.id == orderId);
            if (o == null || o.status != "CONFIRMED")
                return await Task.FromResult(false);

            o.status = "SHIPPING";

            // Gán Serial Numbers nếu có
            if (!string.IsNullOrEmpty(serialNumbersJson))
            {
                try
                {
                    var serialMap = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(serialNumbersJson);
                    if (serialMap != null)
                    {
                        foreach (var oi in o.order_items)
                        {
                            var key = oi.id.ToString();
                            if (serialMap.ContainsKey(key))
                            {
                                foreach (var sn in serialMap[key])
                                {
                                    var serial = new order_item_serial();
                                    serial.order_item_id = oi.id;
                                    serial.serial_number = sn;
                                    _context.order_item_serials.InsertOnSubmit(serial);
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            var history = new order_status_history();
            history.order_id = orderId;
            history.status = "SHIPPING";
            history.note = "Đơn hàng đang được vận chuyển";
            history.created_at = DateTime.UtcNow;
            _context.order_status_histories.InsertOnSubmit(history);

            _context.SubmitChanges();
            return await Task.FromResult(true);
        }

        public async Task<bool> CompleteOrderAsync(int orderId)
        {
            var o = _context.orders.FirstOrDefault(x => x.id == orderId);
            if (o == null || o.status != "SHIPPING")
                return await Task.FromResult(false);

            o.status = "COMPLETED";
            o.payment_status = "PAID";

            // Trừ tồn kho thực tế
            foreach (var oi in o.order_items)
            {
                var inventory = _context.inventories.FirstOrDefault(i => i.variant_id == oi.variant_id && i.warehouse_id == 1);
                if (inventory != null)
                {
                    inventory.quantity_on_hand -= oi.quantity;
                    inventory.quantity_reserved -= oi.quantity;
                }

                var sm = new stock_movement();
                sm.warehouse_id = 1;
                sm.variant_id = oi.variant_id;
                sm.quantity = -oi.quantity;
                sm.movement_type = "SALE_COMPLETED";
                sm.reference_type = "ORDER";
                sm.note = string.Format("Xuất kho hoàn tất đơn hàng {0}", o.code);
                sm.created_at = DateTime.UtcNow;
                _context.stock_movements.InsertOnSubmit(sm);
            }

            var history = new order_status_history();
            history.order_id = orderId;
            history.status = "COMPLETED";
            history.note = "Đơn hàng đã được giao thành công";
            history.created_at = DateTime.UtcNow;
            _context.order_status_histories.InsertOnSubmit(history);

            _context.SubmitChanges();
            return await Task.FromResult(true);
        }

        private OrderResponseDto MapToOrderResponseDto(order o)
        {
            // Parse shipping address
            string recipientName = "";
            string recipientPhone = "";
            string fullAddress = "";
            if (!string.IsNullOrEmpty(o.shipping_address))
            {
                try
                {
                    var addr = JsonConvert.DeserializeObject<dynamic>(o.shipping_address);
                    if (addr != null)
                    {
                        recipientName = (string)(addr.full_name ?? addr.receiver_name ?? "");
                        recipientPhone = (string)(addr.phone ?? addr.receiver_phone ?? "");
                        var street = (string)(addr.address_line ?? addr.street_address ?? "");
                        var ward = (string)(addr.ward ?? "");
                        var district = (string)(addr.district ?? "");
                        var city = (string)(addr.city ?? addr.province ?? "");
                        fullAddress = string.Format("{0}, {1}, {2}, {3}", street, ward, district, city).TrimEnd(',', ' ').Trim();
                    }
                }
                catch { }
            }

            var dto = new OrderResponseDto();
            dto.Id = o.id;
            dto.Code = o.code;
            dto.Subtotal = o.subtotal;
            dto.Discount = o.discount;
            dto.ShippingFee = o.shipping_fee;
            dto.Total = o.total;
            dto.PaymentMethod = o.payment_method;
            dto.PaymentStatus = o.payment_status;
            dto.Status = o.status;
            dto.ShippingAddressJson = o.shipping_address;
            dto.RecipientName = recipientName;
            dto.RecipientPhone = recipientPhone;
            dto.FullAddress = fullAddress;
            dto.Note = o.note;
            dto.CreatedAt = o.created_at;
            dto.UserId = o.user_id;
            dto.UserName = o.user != null ? o.user.full_name : "";
            dto.UserEmail = o.user != null ? o.user.email : "";

            dto.Items = new List<OrderItemDto>();
            if (o.order_items != null)
            {
                foreach (var oi in o.order_items)
                {
                    var itemDto = new OrderItemDto();
                    itemDto.Id = oi.id;
                    itemDto.ProductName = oi.product_name;
                    itemDto.Sku = oi.sku;
                    itemDto.Quantity = oi.quantity;
                    itemDto.UnitPrice = oi.unit_price;
                    itemDto.TotalPrice = oi.total_price;
                    itemDto.ThumbnailUrl = oi.product_variant != null ? oi.product_variant.thumbnail_url : "";
                    itemDto.SerialNumbers = new List<string>();
                    if (oi.order_item_serials != null)
                    {
                        foreach (var s in oi.order_item_serials)
                            itemDto.SerialNumbers.Add(s.serial_number);
                    }
                    dto.Items.Add(itemDto);
                }
            }

            dto.StatusHistory = new List<OrderStatusHistoryDto>();
            if (o.order_status_histories != null)
            {
                foreach (var sh in o.order_status_histories.OrderBy(h => h.created_at))
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
    }
}
