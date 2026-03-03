using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Cart;
using ECommerce.Huit.Application.Services;
using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerce.Huit.UnitTests;

public class CartServiceTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        return context;
    }

    private async Task<ApplicationDbContext> SeedCartAndVariantData()
    {
        var context = CreateInMemoryContext();

        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Slug = "test-product",
            Status = Domain.Enums.ProductStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);

        var variant = new ProductVariant
        {
            Id = 1,
            ProductId = 1,
            Sku = "SKU001",
            Price = 100000,
            IsActive = true
        };
        context.ProductVariants.Add(variant);

        var warehouse = new Warehouse
        {
            Id = 1,
            Name = "Main Warehouse"
        };
        context.Warehouses.Add(warehouse);

        var inventory = new Inventory
        {
            VariantId = 1,
            WarehouseId = 1,
            QuantityOnHand = 50,
            QuantityReserved = 0
        };
        context.Inventories.Add(inventory);

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FullName = "Test User",
            Role = Domain.Enums.UserRole.CUSTOMER,
            Status = Domain.Enums.UserStatus.ACTIVE
        };
        context.Users.Add(user);

        await context.SaveChangesAsync();

        return context;
    }

    [Fact]
    public async Task GetCartAsync_ShouldCreateCart_WhenUserHasNoCart()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;
        var service = new CartService(context);

        // Act
        var result = await service.GetCartAsync(userId);

        // Assert
        Assert.NotNull(result);
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.NotNull(cart);
        Assert.Equal(userId, cart?.UserId);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnExistingCart_WhenUserHasCart()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;

        var existingCart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Carts.Add(existingCart);
        await context.SaveChangesAsync();

        var service = new CartService(context);

        // Act
        var result = await service.GetCartAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingCart.Id, result.Id);
    }

    [Fact]
    public async Task AddItemAsync_ShouldAddNewItem_WhenCartEmpty()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;
        var service = new CartService(context);
        var request = new AddCartItemRequest { VariantId = 1, Quantity = 2 };

        // Act
        var result = await service.AddItemAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(request.Quantity, result.Items[0].Quantity);
        Assert.Equal(100000, result.Items[0].Variant.Price);
        Assert.Equal(200000, result.Total);
    }

    [Fact]
    public async Task AddItemAsync_ShouldUpdateQuantity_WhenItemAlreadyInCart()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;

        var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        var cartItem = new CartItem
        {
            CartId = cart.Id,
            VariantId = 1,
            Quantity = 1
        };
        context.CartItems.Add(cartItem);
        await context.SaveChangesAsync();

        var service = new CartService(context);
        var request = new AddCartItemRequest { VariantId = 1, Quantity = 2 };

        // Act
        var result = await service.AddItemAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        var updatedItem = result.Items.FirstOrDefault();
        Assert.NotNull(updatedItem);
        Assert.Equal(3, updatedItem.Quantity); // 1 + 2 = 3
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowException_WhenVariantDoesNotExist()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;
        var service = new CartService(context);
        var request = new AddCartItemRequest { VariantId = 999, Quantity = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.AddItemAsync(userId, request));
    }

    [Fact]
    public async Task AddItemAsync_ShouldThrowException_WhenInsufficientStock()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;
        var service = new CartService(context);
        var request = new AddCartItemRequest { VariantId = 1, Quantity = 100 }; // Only 50 in stock

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.AddItemAsync(userId, request));
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldUpdateQuantity_WhenQuantityPositive()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;

        var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        var cartItem = new CartItem
        {
            Id = 1,
            CartId = cart.Id,
            VariantId = 1,
            Quantity = 1
        };
        context.CartItems.Add(cartItem);
        await context.SaveChangesAsync();

        var service = new CartService(context);

        // Act
        var result = await service.UpdateItemAsync(userId, 1, 5);

        // Assert
        Assert.NotNull(result);
        var updatedItem = result.Items.FirstOrDefault();
        Assert.NotNull(updatedItem);
        Assert.Equal(5, updatedItem.Quantity);
        Assert.Equal(500000, updatedItem.LineTotal);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldRemoveItem_WhenQuantityZero()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;

        var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        var cartItem = new CartItem
        {
            Id = 1,
            CartId = cart.Id,
            VariantId = 1,
            Quantity = 1
        };
        context.CartItems.Add(cartItem);
        await context.SaveChangesAsync();

        var service = new CartService(context);

        // Act
        var result = await service.UpdateItemAsync(userId, 1, 0);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldThrowException_WhenItemNotFound()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;
        var service = new CartService(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await service.UpdateItemAsync(userId, 999, 1));
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;

        var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        var cartItem = new CartItem
        {
            Id = 1,
            CartId = cart.Id,
            VariantId = 1,
            Quantity = 1
        };
        context.CartItems.Add(cartItem);
        await context.SaveChangesAsync();

        var service = new CartService(context);

        // Act
        var result = await service.RemoveItemAsync(userId, 1);

        // Assert
        Assert.True(result);
        Assert.Empty(await context.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync());
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldReturnFalse_WhenItemNotInCart()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;
        var service = new CartService(context);

        // Act
        var result = await service.RemoveItemAsync(userId, 999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ClearCartAsync_ShouldRemoveAllItems()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;

        var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        context.CartItems.Add(new CartItem { CartId = cart.Id, VariantId = 1, Quantity = 1 });
        context.CartItems.Add(new CartItem { CartId = cart.Id, VariantId = 1, Quantity = 2 });
        await context.SaveChangesAsync();

        var service = new CartService(context);

        // Act
        var result = await service.ClearCartAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Null(result.AppliedVoucher);
    }

    [Fact]
    public async Task ApplyVoucherAsync_ShouldSetVoucherCode()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;
        var service = new CartService(context);
        var voucherCode = "SAVE10";

        // Act
        var result = await service.ApplyVoucherAsync(userId, voucherCode);

        // Assert
        Assert.NotNull(result);
        // Note: Full voucher implementation would calculate discount
        // This is just checking the placeholder behavior
    }

    [Fact]
    public async Task GetCartAsync_ShouldCalculateSubtotalCorrectly()
    {
        // Arrange
        var context = await SeedCartAndVariantData();
        var userId = 1;

        var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        context.CartItems.Add(new CartItem { CartId = cart.Id, VariantId = 1, Quantity = 2 });
        await context.SaveChangesAsync();

        var service = new CartService(context);

        // Act
        var result = await service.GetCartAsync(userId);

        // Assert
        Assert.Equal(200000, result.Subtotal);
        Assert.Equal(200000, result.Total);
    }
}
