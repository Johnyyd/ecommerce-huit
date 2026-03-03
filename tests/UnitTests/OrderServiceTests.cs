using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Order;
using ECommerce.Huit.Application.Services;
using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerce.Huit.UnitTests;

public class OrderServiceTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        return context;
    }

    [Fact]
    public async Task GetOrdersByUserIdAsync_ShouldReturnOrders_WhenUserHasOrders()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1;

        var order1 = new Order
        {
            UserId = userId,
            Code = "ORD001",
            Subtotal = 1000000,
            Discount = 0,
            Total = 1000000,
            PaymentMethod = "COD",
            PaymentStatus = Domain.Enums.PaymentStatus.PENDING,
            Status = Domain.Enums.OrderStatus.CONFIRMED,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductName = "Test Product",
                    Sku = "SKU001",
                    Quantity = 2,
                    UnitPrice = 500000,
                    TotalPrice = 1000000
                }
            }
        };

        var order2 = new Order
        {
            UserId = userId,
            Code = "ORD002",
            Subtotal = 2000000,
            Discount = 100000,
            Total = 1900000,
            PaymentMethod = "VNPAY",
            PaymentStatus = Domain.Enums.PaymentStatus.PAID,
            Status = Domain.Enums.OrderStatus.COMPLETED,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.Orders.Add(order1);
        context.Orders.Add(order2);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.GetOrdersByUserIdAsync(userId, page: 1, pageSize: 20);

        // Assert
        Assert.NotNull(result);
        var orderList = result.ToList();
        Assert.Equal(2, orderList.Count);
        Assert.Equal("ORD001", orderList[0].Code);
        Assert.Equal("ORD002", orderList[1].Code);
    }

    [Fact]
    public async Task GetOrdersByUserIdAsync_ShouldPaginateCorrectly()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userId = 1;

        for (int i = 1; i <= 5; i++)
        {
            var order = new Order
            {
                UserId = userId,
                Code = $"ORD{i:000}",
                Subtotal = i * 100000,
                Total = i * 100000,
                PaymentMethod = "COD",
                PaymentStatus = Domain.Enums.PaymentStatus.PENDING,
                Status = Domain.Enums.OrderStatus.PENDING,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            };
            context.Orders.Add(order);
        }
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act - get page 1 with pageSize 2
        var result = await service.GetOrdersByUserIdAsync(userId, page: 1, pageSize: 2);

        // Assert
        var orderList = result.ToList();
        Assert.Equal(2, orderList.Count);
        Assert.Equal("ORD005", orderList[0].Code); // newest first
        Assert.Equal("ORD004", orderList[1].Code);
    }

    [Fact]
    public async Task GetOrderByCodeAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var orderCode = "TEST001";
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = 1,
            Code = orderCode,
            Subtotal = 1000000,
            Discount = 0,
            ShippingFee = 30000,
            Total = 1030000,
            PaymentMethod = "VNPAY",
            PaymentStatus = Domain.Enums.PaymentStatus.PAID,
            Status = Domain.Enums.OrderStatus.SHIPPING,
            ShippingAddress = "{\"address\":\"123 Test St\"}",
            Note = "Test order",
            CreatedAt = DateTime.UtcNow,
            StatusHistories = new List<OrderStatusHistory>
            {
                new OrderStatusHistory
                {
                    OrderId = orderId,
                    Status = Domain.Enums.OrderStatus.PENDING.ToString(),
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new OrderStatusHistory
                {
                    OrderId = orderId,
                    Status = Domain.Enums.OrderStatus.CONFIRMED.ToString(),
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new OrderStatusHistory
                {
                    OrderId = orderId,
                    Status = Domain.Enums.OrderStatus.SHIPPING.ToString(),
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.GetOrderByCodeAsync(orderCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderCode, result.Code);
        Assert.Equal("SHIPPING", result.Status);
        Assert.Equal(3, result.StatusHistory.Count);
        Assert.Equal("{\"address\":\"123 Test St\"}", result.ShippingAddressJson);
    }

    [Fact]
    public async Task GetOrderByCodeAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new OrderService(context);

        // Act
        var result = await service.GetOrderByCodeAsync("NONEXISTENT");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldReturnTrue_WhenOrderIsPending()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = 1,
            Code = "ORD001",
            Status = Domain.Enums.OrderStatus.PENDING,
            Subtotal = 100000,
            Total = 100000
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.CancelOrderAsync(orderId, "Test cancellation");

        // Assert
        Assert.True(result);
        var updatedOrder = await context.Orders.FindAsync(orderId);
        Assert.Equal(Domain.Enums.OrderStatus.CANCELLED, updatedOrder?.Status);
        Assert.Contains("Test cancellation", updatedOrder?.Note);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderCannotBeCancelled()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = 1,
            Code = "ORD001",
            Status = Domain.Enums.OrderStatus.COMPLETED, // Cannot cancel completed order
            Subtotal = 100000,
            Total = 100000
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.CancelOrderAsync(orderId, "Test cancellation");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ConfirmOrderAsync_ShouldReturnTrue_WhenOrderIsPending()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var orderId = 1;
        var staffId = 10;

        var order = new Order
        {
            Id = orderId,
            UserId = 1,
            Code = "ORD001",
            Status = Domain.Enums.OrderStatus.PENDING,
            Subtotal = 100000,
            Total = 100000
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.ConfirmOrderAsync(orderId, staffId);

        // Assert
        Assert.True(result);
        var updatedOrder = await context.Orders.FindAsync(orderId);
        Assert.Equal(Domain.Enums.OrderStatus.CONFIRMED, updatedOrder?.Status);

        var history = await context.OrderStatusHistories
            .FirstOrDefaultAsync(h => h.OrderId == orderId && h.Status == Domain.Enums.OrderStatus.CONFIRMED.ToString());
        Assert.NotNull(history);
        Assert.Equal(staffId, history?.ChangedBy);
    }

    [Fact]
    public async Task ConfirmOrderAsync_ShouldReturnFalse_WhenOrderNotPending()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = 1,
            Code = "ORD001",
            Status = Domain.Enums.OrderStatus.CONFIRMED, // Already confirmed
            Subtotal = 100000,
            Total = 100000
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.ConfirmOrderAsync(orderId, null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CompleteOrderAsync_ShouldReturnTrue_WhenOrderIsShipping()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = 1,
            Code = "ORD001",
            Status = Domain.Enums.OrderStatus.SHIPPING,
            Subtotal = 100000,
            Total = 100000
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.CompleteOrderAsync(orderId);

        // Assert
        Assert.True(result);
        var updatedOrder = await context.Orders.FindAsync(orderId);
        Assert.Equal(Domain.Enums.OrderStatus.COMPLETED, updatedOrder?.Status);
    }

    [Fact]
    public async Task CompleteOrderAsync_ShouldReturnFalse_WhenOrderNotShipping()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var orderId = 1;

        var order = new Order
        {
            Id = orderId,
            UserId = 1,
            Code = "ORD001",
            Status = Domain.Enums.OrderStatus.PENDING,
            Subtotal = 100000,
            Total = 100000
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);

        // Act
        var result = await service.CompleteOrderAsync(orderId);

        // Assert
        Assert.False(result);
    }
}
