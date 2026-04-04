using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Cart;
using ECommerce.Huit.Application.DTOs.Product;
using ECommerce.Huit.Domain.Entities;

namespace ECommerce.Huit.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IApplicationDbContext _context;

        public CartService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetCartByUserIdAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items.Select(ci => ci.Variant.Product))
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Create new cart if not exists
                cart = new Cart();
                cart.UserId = userId;
                cart.CreatedAt = DateTime.UtcNow;
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return MapToCartDto(cart);
        }

        public async Task<bool> AddItemToCartAsync(int userId, AddCartItemRequest request)
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Check variant exists and is active
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == request.VariantId && v.IsActive);

            if (variant == null)
                throw new ArgumentException("Sản phẩm không tồn tại hoặc không khả dụng");

            // Check stock
            var inventoryList = await _context.Inventories
                .Where(i => i.VariantId == request.VariantId)
                .ToListAsync();
            
            var availableStock = inventoryList.Sum(i => i.QuantityOnHand - i.QuantityReserved);

            if (availableStock < request.Quantity)
                throw new InvalidOperationException("Số lượng tồn kho không đủ");

            // Check if item already in cart
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.VariantId == request.VariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                var cartItem = new CartItem();
                cartItem.CartId = cart.Id;
                cartItem.VariantId = request.VariantId;
                cartItem.Quantity = request.Quantity;
                cartItem.CreatedAt = DateTime.UtcNow;
                _context.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);

            if (cartItem == null)
                throw new ArgumentException("Sản phẩm không có trong giỏ hàng");

            if (quantity <= 0)
            {
                // Remove item
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                // Check stock
                var inventoryList = await _context.Inventories
                    .Where(i => i.VariantId == cartItem.VariantId)
                    .ToListAsync();

                var availableStock = inventoryList.Sum(i => i.QuantityOnHand - i.QuantityReserved);

                if (availableStock < quantity)
                    throw new InvalidOperationException("Số lượng tồn kho không đủ");

                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int cartItemId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);

            if (cartItem == null) return false;

            _context.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var items = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            foreach (var item in items)
            {
                _context.CartItems.Remove(item);
            }

            cart.VoucherCode = null;
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart();
                cart.UserId = userId;
                cart.CreatedAt = DateTime.UtcNow;
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private CartDto MapToCartDto(Cart cart)
        {
            decimal subtotal = 0;
            if (cart.Items != null)
            {
                foreach (var item in cart.Items)
                {
                    subtotal += item.Variant.Price * item.Quantity;
                }
            }

            var discount = 0m;
            var total = subtotal - discount;

            var dto = new CartDto();
            dto.Id = cart.Id;
            dto.Subtotal = subtotal;
            dto.Discount = discount;
            dto.Total = total;
            dto.Items = new List<CartItemDto>();

            if (cart.Items != null)
            {
                foreach (var ci in cart.Items)
                {
                    var itemDto = new CartItemDto();
                    itemDto.Id = ci.Id;
                    itemDto.Quantity = ci.Quantity;
                    itemDto.LineTotal = ci.Variant.Price * ci.Quantity;

                    var variantDto = new ProductVariantDto();
                    variantDto.Id = ci.Variant.Id;
                    variantDto.Sku = ci.Variant.Sku;
                    variantDto.VariantName = ci.Variant.VariantName;
                    variantDto.Price = ci.Variant.Price;
                    variantDto.OriginalPrice = ci.Variant.OriginalPrice;
                    variantDto.ThumbnailUrl = ci.Variant.ThumbnailUrl;

                    itemDto.Variant = variantDto;
                    dto.Items.Add(itemDto);
                }
            }

            return dto;
        }
    }
}
