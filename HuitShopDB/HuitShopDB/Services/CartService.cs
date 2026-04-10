using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Cart;
using HuitShopDB.Models.DTOs.Product;
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
                // Create new cart if not exists
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

            // Check variant exists and is active
            var variant = _context.product_variants
                .FirstOrDefault(v => v.id == request.VariantId && v.is_active == true);

            if (variant == null)
                throw new ArgumentException("Sản phẩm không tồn tại hoặc không khả dụng");

            // Check stock
            var inventoryList = _context.inventories
                .Where(i => i.variant_id == request.VariantId)
                .ToList();
            
            var availableStock = inventoryList.Sum(i => i.quantity_on_hand - i.quantity_reserved);

            if (availableStock < request.Quantity)
                throw new InvalidOperationException("Số lượng tồn kho không đủ");

            // Check if item already in cart
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
                // Remove item
                _context.cart_items.DeleteOnSubmit(cartItem);
            }
            else
            {
                // Check stock
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

        private async Task<cart> GetOrCreateCartAsync(int userId)
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

