using System.Net.Http.Json;
using System.Text.Json;
using ECommerce.Huit.API;
using ECommerce.Huit.Application.DTOs.Order;
using ECommerce.Huit.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.Huit.IntegrationTests;

public class OrderEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrderEndpointTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("OrderTestDb");
                });
            });
        });
    }

    private async Task<(int userId, int variantId, string authToken)> SeedOrderData(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Email = "orderuser@example.com",
            FullName = "Order User",
            Role = Domain.Enums.UserRole.CUSTOMER,
            Status = Domain.Enums.UserStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Order Test Product",
            Slug = "order-test-product",
            Status = Domain.Enums.ProductStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,
            Sku = "ORDER-001",
            Price = 500000,
            IsActive = true
        };
        context.ProductVariants.Add(variant);
        await context.SaveChangesAsync();

        var warehouse = new Warehouse { Name = "Order Warehouse" };
        context.Warehouses.Add(warehouse);
        await context.SaveChangesAsync();

        var inventory = new Inventory
        {
            VariantId = variant.Id,
            WarehouseId = warehouse.Id,
            QuantityOnHand = 10,
            QuantityReserved = 0
        };
        context.Inventories.Add(inventory);
        await context.SaveChangesAsync();

        // Create auth token for user (we'll use JWTBearer mock or just pass userId as query)
        return (user.Id, variant.Id, string.Empty);
    }

    [Fact]
    public async Task CreateOrder_ShouldCreateOrder_WhenCartHasItems()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, variantId, _) = await SeedOrderData(_factory);

        // First add item to cart (via direct context manipulation for simplicity)
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                VariantId = variantId,
                Quantity = 2
            };
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();
        }

        var request = new CreateOrderRequest
        {
            ShippingAddressJson = "{\"address\":\"123 Street\"}",
            PaymentMethod = "COD",
            Note = "Test order"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/orders?userId={userId}", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var order = JsonDocument.Parse(content).RootElement;

        Assert.Equal("COD", order.GetProperty("paymentMethod").GetString());
        Assert.Equal("PENDING", order.GetProperty("status").GetString());
        Assert.Equal(0, order.GetProperty("discount").GetDecimal());
        Assert.Equal(0, order.GetProperty("shippingFee").GetDecimal());
        Assert.Equal(1000000, order.GetProperty("total").GetDecimal()); // 2 * 500000
        Assert.Single(order.GetProperty("items"));
    }

    [Fact]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenCartEmpty()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, _, _) = await SeedOrderData(_factory);

        // Create empty cart
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();
        }

        var request = new CreateOrderRequest
        {
            ShippingAddressJson = "{\"address\":\"123 Street\"}",
            PaymentMethod = "COD"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/orders?userId={userId}", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrdersByUserId_ShouldReturnUserOrders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, _, _) = await SeedOrderData(_factory);

        // Create some orders directly in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var order1 = new Order
            {
                UserId = userId,
                Code = "ORD001",
                Subtotal = 1000000,
                Total = 1000000,
                PaymentMethod = "COD",
                Status = Domain.Enums.OrderStatus.CONFIRMED,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            var order2 = new Order
            {
                UserId = userId,
                Code = "ORD002",
                Subtotal = 2000000,
                Total = 2000000,
                PaymentMethod = "VNPAY",
                Status = Domain.Enums.OrderStatus.COMPLETED,
                CreatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order1);
            context.Orders.Add(order2);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/orders?userId={userId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonDocument.Parse(content).RootElement;

        Assert.Equal(2, orders.GetArrayLength());
        Assert.Equal("ORD002", orders[0].GetProperty("code").GetString()); // newest first
        Assert.Equal("ORD001", orders[1].GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetOrderByCode_ShouldReturnOrder_WhenExistsForUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, _, _) = await SeedOrderData(_factory);
        var orderCode = "TESTORDER123";

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var order = new Order
            {
                UserId = userId,
                Code = orderCode,
                Subtotal = 500000,
                Total = 500000,
                PaymentMethod = "COD",
                Status = Domain.Enums.OrderStatus.SHIPPING,
                ShippingAddress = "{\"address\":\"456 Road\"}",
                CreatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/orders/{orderCode}?userId={userId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var order = JsonDocument.Parse(content).RootElement;

        Assert.Equal(orderCode, order.GetProperty("code").GetString());
        Assert.Equal("SHIPPING", order.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetOrderByCode_ShouldReturnNotFound_WhenOrderNotExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, _, _) = await SeedOrderData(_factory);

        // Act
        var response = await client.GetAsync($"/api/orders/NONEXISTENT?userId={userId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_ShouldCancelPendingOrder()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, variantId, _) = await SeedOrderData(_factory);

        // Create order via direct DB insert for simplicity
        Order createdOrder = null;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            context.Carts.Add(cart);
            await context.SaveChangesAsync();

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                VariantId = variantId,
                Quantity = 1
            };
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();

            // TODO: Normally would call POST /api/orders to create order
            // For now, create order directly in DB with PENDING status
            var order = new Order
            {
                UserId = userId,
                Code = "CANCELTEST",
                Subtotal = 500000,
                Total = 500000,
                PaymentMethod = "COD",
                Status = Domain.Enums.OrderStatus.PENDING,
                CreatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            createdOrder = order;
        }

        // Act
        var response = await client.PutAsync($"/api/orders/{createdOrder.Id}/cancel?userId={userId}&reason=Test+cancellation", null);

        // Assert
        response.EnsureSuccessStatusCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var order = await context.Orders.FindAsync(createdOrder.Id);
            Assert.Equal(Domain.Enums.OrderStatus.CANCELLED, order?.Status);
            Assert.Contains("Test cancellation", order?.Note);
        }
    }

    [Fact]
    public async Task CancelOrder_ShouldReturnBadRequest_WhenOrderCannotBeCancelled()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, _, _) = await SeedOrderData(_factory);

        Order completedOrder = null;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var order = new Order
            {
                UserId = userId,
                Code = "COMPLETED123",
                Status = Domain.Enums.OrderStatus.COMPLETED,
                Subtotal = 500000,
                Total = 500000,
                PaymentMethod = "COD",
                CreatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();
            completedOrder = order;
        }

        // Act
        var response = await client.PutAsync($"/api/orders/{completedOrder.Id}/cancel?userId={userId}&reason=Should+fail", null);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ShouldPaginateCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, _, _) = await SeedOrderData(_factory);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            for (int i = 1; i <= 5; i++)
            {
                var order = new Order
                {
                    UserId = userId,
                    Code = $"PAGETEST{i:000}",
                    Status = Domain.Enums.OrderStatus.PENDING,
                    Subtotal = i * 100000,
                    Total = i * 100000,
                    PaymentMethod = "COD",
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                };
                context.Orders.Add(order);
            }
            await context.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/orders?userId={userId}&page=1&pageSize=2");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var orders = JsonDocument.Parse(content).RootElement;

        Assert.Equal(2, orders.GetArrayLength());
        var pagination = JsonDocument.Parse(content).RootElement.GetProperty("pagination");
        Assert.Equal(1, pagination.GetProperty("page").GetInt32());
        Assert.Equal(5, pagination.GetProperty("totalItems").GetInt32());
        Assert.True(pagination.GetProperty("hasNext").GetBoolean());
    }
}
