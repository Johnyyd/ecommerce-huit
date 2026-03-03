using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Product;
using ECommerce.Huit.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.UnitTests;

public class ProductServiceTests
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
    public async Task GetProductsAsync_ShouldReturnActiveProducts_WhenNoFilter()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var category = new Category { Name = "Test Category", Slug = "test-category", IsActive = true };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            CategoryId = category.Id,
            Status = Domain.Enums.ProductStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Sku = "TEST-001",
                    Price = 1000000,
                    IsActive = true,
                    Images = new List<ProductImage>()
                }
            }
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var query = new ProductQueryParams { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetProductsAsync(query);

        // Assert
        Assert.NotNull(result);
        var productList = result.ToList();
        Assert.Single(productList);
        Assert.Equal("Test Product", productList[0].Name);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenProductExistsAndActive()
    {
        // Arrange
        var context = CreateInMemoryContext();

        var product = new Product
        {
            Id = 1,
            Name = "iPhone 15",
            Slug = "iphone-15",
            Status = Domain.Enums.ProductStatus.ACTIVE,
            CreatedAt = DateTime.UtcNow,
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = 1,
                    Sku = "IP15-001",
                    Price = 28990000,
                    IsActive = true,
                    Inventories = new List<Inventory>
                    {
                        new Inventory { QuantityOnHand = 10, QuantityReserved = 0 }
                    }
                }
            }
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = await service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("iPhone 15", result.Name);
        Assert.Single(result.Variants);
        Assert.Equal(10, result.Variants[0].QuantityAvailable);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductIsDraft()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var product = new Product
        {
            Id = 1,
            Name = "Draft Product",
            Slug = "draft-product",
            Status = Domain.Enums.ProductStatus.DRAFT
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = await service.GetProductByIdAsync(1);

        // Assert
        Assert.Null(result);
    }
}
