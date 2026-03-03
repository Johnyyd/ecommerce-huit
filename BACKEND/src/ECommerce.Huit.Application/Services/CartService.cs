using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Cart;
using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.Application.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            // Create new cart if not exists
            cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return MapToCartDto(cart);
    }

    public async Task<CartDto> AddItemAsync(int userId, AddCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);

        // Check variant exists and is active
        var variant = await _context.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == request.VariantId && v.IsActive);

        if (variant == null)
            throw new ArgumentException("Sản phẩm không tồn tại hoặc không khả dụng");

        // Check stock
        var availableStock = await _context.Inventories
            .Where(i => i.VariantId == request.VariantId)
            .SumAsync(i => i.QuantityOnHand - i.QuantityReserved);

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
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                VariantId = request.VariantId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow
            };
            _context.CartItems.Add(cartItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateItemAsync(int userId, int itemId, int quantity)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.CartId == cart.Id);

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
            var availableStock = await _context.Inventories
                .Where(i => i.VariantId == cartItem.VariantId)
                .SumAsync(i => i.QuantityOnHand - i.QuantityReserved);

            if (availableStock < quantity)
                throw new InvalidOperationException("Số lượng tồn kho không đủ");

            cartItem.Quantity = quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<bool> RemoveItemAsync(int userId, int itemId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.CartId == cart.Id);

        if (cartItem == null) return false;

        _context.CartItems.Remove(cartItem);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CartDto> ApplyVoucherAsync(int userId, string voucherCode)
    {
        var cart = await GetOrCreateCartAsync(userId);

        // TODO: Validate voucher and calculate discount
        // For now: simple placeholder
        cart.VoucherCode = voucherCode;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<CartDto> ClearCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var items = await _context.CartItems
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync();

        _context.CartItems.RemoveRange(items);
        cart.VoucherCode = null;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    private async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    private CartDto MapToCartDto(Cart cart)
    {
        var subtotal = cart.Items.Sum(i => i.Variant.Price * i.Quantity);
        // TODO: calculate discount based on voucher
        var discount = 0m;
        var total = subtotal - discount;

        return new CartDto
        {
            Id = cart.Id,
            Items = cart.Items.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                Variant = new ProductVariantDto
                {
                    Id = ci.Variant.Id,
                    Sku = ci.Variant.Sku,
                    VariantName = ci.Variant.VariantName,
                    Price = ci.Variant.Price,
                    OriginalPrice = ci.Variant.OriginalPrice,
                    ThumbnailUrl = ci.Variant.ThumbnailUrl
                },
                Quantity = ci.Quantity,
                LineTotal = ci.Variant.Price * ci.Quantity
            }).ToList(),
            Subtotal = subtotal,
            Discount = discount,
            Total = total
            // AppliedVoucher: TODO
        };
    }
}
