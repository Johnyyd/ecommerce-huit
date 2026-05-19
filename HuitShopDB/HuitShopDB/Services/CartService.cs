using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Cart;
using HuitShopDB.Models.DTOs.Product;
using HuitShopDB.Models.DTOs.Voucher;
using HuitShopDB.Models;

namespace HuitShopDB.Services
{
    public class CartService : ICartService
    {
        private readonly HuitShopDBDataContext _context;

        public CartService()
        {
            _context = new HuitShopDBDataContext();
        }

        public async Task<CartDto> GetCartByUserIdAsync(int userId)
        {
            var cart = _context.carts
                .FirstOrDefault(c => c.user_id == userId);

            if (cart == null)
            {
                cart = new cart();
                cart.user_id = userId;
                cart.created_at = DateTime.UtcNow;
                _context.carts.InsertOnSubmit(cart);
                _context.SubmitChanges();
            }

            return await Task.FromResult(MapToCartDto(cart));
        }

        public async Task<bool> AddItemToCartAsync(int userId, AddCartItemRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var variant = _context.product_variants
                .FirstOrDefault(v => v.id == request.VariantId && v.is_active == true);

            if (variant == null)
                throw new ArgumentException("Sản phẩm không tồn tại hoặc không khả dụng");

            var inventoryList = _context.inventories
                .Where(i => i.variant_id == request.VariantId)
                .ToList();

            var availableStock = inventoryList.Sum(i => i.quantity_on_hand - i.quantity_reserved);

            if (availableStock < request.Quantity)
                throw new InvalidOperationException("Số lượng tồn kho không đủ");

            var existingItem = _context.cart_items
                .FirstOrDefault(ci => ci.cart_id == cart.id && ci.variant_id == request.VariantId);

            if (existingItem != null)
            {
                existingItem.quantity += request.Quantity;
            }
            else
            {
                var cartItem = new cart_item();
                cartItem.cart_id = cart.id;
                cartItem.variant_id = request.VariantId;
                cartItem.quantity = request.Quantity;
                _context.cart_items.InsertOnSubmit(cartItem);
            }

            cart.updated_at = DateTime.UtcNow;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = _context.cart_items
                .FirstOrDefault(ci => ci.id == cartItemId && ci.cart_id == cart.id);

            if (cartItem == null)
                throw new ArgumentException("Sản phẩm không có trong giỏ hàng");

            if (quantity <= 0)
            {
                _context.cart_items.DeleteOnSubmit(cartItem);
            }
            else
            {
                var inventoryList = _context.inventories
                    .Where(i => i.variant_id == cartItem.variant_id)
                    .ToList();

                var availableStock = inventoryList.Sum(i => i.quantity_on_hand - i.quantity_reserved);

                if (availableStock < quantity)
                    throw new InvalidOperationException("Số lượng tồn kho không đủ");

                cartItem.quantity = quantity;
            }

            cart.updated_at = DateTime.UtcNow;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = _context.cart_items
                .FirstOrDefault(ci => ci.id == cartItemId && ci.cart_id == cart.id);

            if (cartItem == null) return await Task.FromResult(false);

            _context.cart_items.DeleteOnSubmit(cartItem);
            cart.updated_at = DateTime.UtcNow;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var items = _context.cart_items
                .Where(ci => ci.cart_id == cart.id)
                .ToList();

            foreach (var item in items)
            {
                _context.cart_items.DeleteOnSubmit(item);
            }

            cart.voucher_code = null;
            cart.updated_at = DateTime.UtcNow;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<ValidateVoucherResponse> ApplyVoucherAsync(int userId, string code)
        {
            var response = new ValidateVoucherResponse();

            if (string.IsNullOrWhiteSpace(code))
            {
                response.Valid = false;
                response.Reason = "Mã voucher không hợp lệ";
                return await Task.FromResult(response);
            }

            var cart = await GetOrCreateCartAsync(userId);

            decimal subtotal = 0;
            foreach (var ci in cart.cart_items)
            {
                subtotal += ci.quantity * (ci.product_variant != null ? ci.product_variant.price : 0m);
            }

            var now = DateTime.UtcNow;
            var voucher = _context.vouchers.FirstOrDefault(v =>
                v.code == code.Trim().ToUpper() &&
                v.is_active == true &&
                v.start_date <= now &&
                v.end_date >= now &&
                (v.usage_limit == null || v.usage_count < v.usage_limit) &&
                subtotal >= v.min_order_value);

            if (voucher == null)
            {
                var existingVoucher = _context.vouchers.FirstOrDefault(v => v.code == code.Trim().ToUpper());
                if (existingVoucher == null)
                    response.Reason = "Mã voucher không tồn tại";
                else if (existingVoucher.is_active != true)
                    response.Reason = "Voucher đã bị vô hiệu hóa";
                else if (existingVoucher.end_date < now)
                    response.Reason = "Voucher đã hết hạn";
                else if (subtotal < existingVoucher.min_order_value)
                    response.Reason = string.Format("Đơn hàng tối thiểu {0:N0}đ để dùng voucher này", existingVoucher.min_order_value);
                else
                    response.Reason = "Voucher đã hết lượt sử dụng";

                response.Valid = false;
                return await Task.FromResult(response);
            }

            cart.voucher_code = voucher.code;
            cart.updated_at = DateTime.UtcNow;
            _context.SubmitChanges();

            response.Valid = true;
            response.Voucher = new VoucherDto
            {
                Id = voucher.id,
                Code = voucher.code,
                Name = voucher.name,
                DiscountType = voucher.discount_type,
                DiscountValue = voucher.discount_value,
                MaxDiscountAmount = voucher.max_discount_amount,
                MinOrderValue = voucher.min_order_value
            };
            return await Task.FromResult(response);
        }

        public async Task<bool> RemoveVoucherAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            cart.voucher_code = null;
            cart.updated_at = DateTime.UtcNow;
            _context.SubmitChanges();
            return await Task.FromResult(true);
        }

        private async Task<cart> GetOrCreateCartAsync(int userId)
        {
            var cart = _context.carts.FirstOrDefault(c => c.user_id == userId);

            if (cart == null)
            {
                cart = new cart();
                cart.user_id = userId;
                cart.created_at = DateTime.UtcNow;
                _context.carts.InsertOnSubmit(cart);
                _context.SubmitChanges();
            }

            return await Task.FromResult(cart);
        }

        private CartDto MapToCartDto(cart cart)
        {
            decimal subtotal = 0;
            if (cart.cart_items != null)
            {
                foreach (var item in cart.cart_items)
                {
                    subtotal += item.product_variant.price * item.quantity;
                }
            }

            var discount = 0m;
            var total = subtotal - discount;

            var dto = new CartDto();
            dto.Id = cart.id;
            dto.Subtotal = subtotal;
            dto.Discount = discount;
            dto.Total = total;
            dto.Items = new List<CartItemDto>();

            if (cart.cart_items != null)
            {
                foreach (var ci in cart.cart_items)
                {
                    var itemDto = new CartItemDto();
                    itemDto.Id = ci.id;
                    itemDto.Quantity = ci.quantity;
                    itemDto.LineTotal = ci.product_variant.price * ci.quantity;

                    var variantDto = new ProductVariantDto();
                    variantDto.Id = ci.product_variant.id;
                    variantDto.Sku = ci.product_variant.sku;
                    variantDto.VariantName = ci.product_variant.variant_name;
                    variantDto.Price = ci.product_variant.price;
                    variantDto.OriginalPrice = ci.product_variant.original_price;
                    variantDto.ThumbnailUrl = ci.product_variant.thumbnail_url;

                    itemDto.Variant = variantDto;
                    dto.Items.Add(itemDto);
                }
            }

            return dto;
        }
    }
}
