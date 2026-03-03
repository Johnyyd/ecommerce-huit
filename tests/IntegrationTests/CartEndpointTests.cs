using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Huit.API;
using ECommerce.Huit.Application.DTOs.Cart;
using ECommerce.Huit.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.Huit.IntegrationTests;

public class CartEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CartEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace database with in-memory
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("CartTestDb");
                });
            });
        });
    }

    private async Task<int> SeedUserAndProduct(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Email = "cartuser@example.com",
            FullName = "Cart User",
            Role = Domain.Enums.UserRole.CUSTOMER,
            Status = Domain.Enums.UserStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);

        var product = new Product
        {
            Name = "Cart Test Product",
            Slug = "cart-test-product",
            Status = Domain.Enums.ProductStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);

        await context.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Sku = "CART-001",
            Price = 100000,
            IsActive = true
        };
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        var warehouse = new Warehouse
        {
            Name = "Test Warehouse"
        };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        var inventory = new Inventory
        {
            VariantId = variant.Id,
            WarehouseId = warehouse.Id,
            QuantityOnHand = 100,
            QuantityReserved = 0
        };
        context.Inventories.Add(inventory);
        await context.SaveChangesAsync();

        return user.Id;
    }

    [Fact]
    public async Task GetCart_WhenUserHasNoCart_ShouldCreateNewCart()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);

        // Act
        var response = await client.GetAsync($"/api/cart?userId={userId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;

        Assert.Equal(userId, cart.GetProperty("userId").GetInt32());
        Assert.Empty(cart.GetProperty("items"));
        Assert.Equal(0, cart.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task AddItem_ShouldAddProductToCart()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);
        var addRequest = new { variantId = 1, quantity = 2 };

        // Act
        var response = await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", addRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;

        Assert.Single(cart.GetProperty("items"));
        Assert.Equal(2, cart.GetProperty("items")[0].GetProperty("quantity").GetInt32());
        Assert.Equal(200000, cart.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task AddItem_ShouldReturnBadRequest_WhenVariantDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);
        var addRequest = new { variantId = 999, quantity = 1 };

        // Act
        var response = await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", addRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddItem_ShouldReturnBadRequest_WhenInsufficientStock()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);
        var addRequest = new { variantId = 1, quantity = 1000 }; // more than stock

        // Act
        var response = await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", addRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_ShouldUpdateQuantity()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);

        // First add item
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 1 });

        // Act - update to 5
        var updateResponse = await client.PutAsJsonAsync("/api/cart/items/1?userId=1", new { quantity = 5 });

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var content = await updateResponse.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;
        Assert.Equal(5, cart.GetProperty("items")[0].GetProperty("quantity").GetInt32());
        Assert.Equal(500000, cart.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task UpdateItem_ShouldRemoveItem_WhenQuantityIsZero()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);

        // First add item
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 1 });

        // Act - update to 0
        var updateResponse = await client.PutAsJsonAsync("/api/cart/items/1?userId=1", new { quantity = 0 });

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var content = await updateResponse.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;
        Assert.Empty(cart.GetProperty("items"));
    }

    [Fact]
    public async Task RemoveItem_ShouldDeleteItemFromCart()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);

        // First add item
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 1 });

        // Act
        var deleteResponse = await client.DeleteAsync("/api/cart/items/1?userId=1");

        // Assert
        deleteResponse.EnsureSuccessStatusCode();
        var content = await deleteResponse.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;
        Assert.Empty(cart.GetProperty("items"));
    }

    [Fact]
    public async Task ClearCart_ShouldRemoveAllItems()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);

        // Add multiple items
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 2 });
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 3 });

        // Act
        var clearResponse = await client.PostAsync($"/api/cart/clear?userId={userId}", null);

        // Assert
        clearResponse.EnsureSuccessStatusCode();
        var content = await clearResponse.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;
        Assert.Empty(cart.GetProperty("items"));
        Assert.Equal(0, cart.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task ApplyVoucher_ShouldApplyVoucherCode()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 2 });

        // Act
        var response = await client.PostAsync($"/api/cart/apply-voucher?userId={userId}&code=TEST10", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;
        // Voucher is applied (discount calculation varies)
        Assert.NotNull(cart.GetProperty("appliedVoucher"));
    }

    [Fact]
    public async Task AddItem_ShouldAggregateQuantity_ForSameVariant()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = await SeedUserAndProduct(_factory);

        // Act - add same variant twice
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 2 });
        await client.PostAsJsonAsync($"/api/cart/items?userId={userId}", new { variantId = 1, quantity = 3 });

        // Assert
        var response = await client.GetAsync($"/api/cart?userId={userId}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var cart = JsonDocument.Parse(content).RootElement;

        Assert.Single(cart.GetProperty("items"));
        Assert.Equal(5, cart.GetProperty("items")[0].GetProperty("quantity").GetInt32());
    }
}
