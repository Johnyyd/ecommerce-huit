using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Product;
using ECommerce.Huit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Huit.Application.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _context;

    public ProductService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductListDto>> GetProductsAsync(ProductQueryParams query)
    {
        var productsQuery = _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Where(p => p.Status == Domain.Enums.ProductStatus.ACTIVE)
            .AsQueryable();

        // Apply filters
        if (query.CategoryId.HasValue)
            productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.BrandId.HasValue)
            productsQuery = productsQuery.Where(p => p.BrandId == query.BrandId.Value);

        if (query.MinPrice.HasValue)
            productsQuery = productsQuery.Where(p => p.Variants.Any(v => v.Price >= query.MinPrice.Value));

        if (query.MaxPrice.HasValue)
            productsQuery = productsQuery.Where(p => p.Variants.Any(v => v.Price <= query.MaxPrice.Value));

        if (!string.IsNullOrEmpty(query.Search))
            productsQuery = productsQuery.Where(p =>
                p.Name.Contains(query.Search) ||
                (p.Description != null && p.Description.Contains(query.Search)));

        if (query.InStockOnly)
            productsQuery = productsQuery.Where(p =>
                p.Variants.Any(v => v.Inventories.Any(i => i.QuantityOnHand - i.QuantityReserved > 0)));

        // Apply sorting
        productsQuery = query.SortBy switch
        {
            "price_asc" => productsQuery.OrderBy(p => p.Variants.Min(v => v.Price)),
            "price_desc" => productsQuery.OrderByDescending(p => p.Variants.Max(v => v.Price)),
            "newest" => productsQuery.OrderByDescending(p => p.CreatedAt),
            "name" => productsQuery.OrderBy(p => p.Name),
            _ => productsQuery.OrderByDescending(p => p.CreatedAt)
        };

        // Pagination
        var products = await productsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        // Map to DTO
        var result = products.Select(MapToProductListDto).ToList();
        return result;
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Inventories)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == Domain.Enums.ProductStatus.ACTIVE);

        if (product == null) return null;

        return MapToProductDetailDto(product);
    }

    public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Inventories)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == Domain.Enums.ProductStatus.ACTIVE);

        if (product == null) return null;

        return MapToProductDetailDto(product);
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        // Build tree
        var categoryDict = categories.ToDictionary(c => c.Id, c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            ParentId = c.ParentId,
            Children = new List<CategoryDto>()
        });

        foreach (var category in categories)
        {
            if (category.ParentId.HasValue && categoryDict.ContainsKey(category.ParentId.Value))
            {
                categoryDict[category.ParentId.Value].Children.Add(categoryDict[category.Id]);
            }
        }

        // Return root categories (ParentId is null)
        return categoryDict.Values.Where(c => !c.ParentId.HasValue).ToList();
    }

    public async Task<IEnumerable<BrandDto>> GetBrandsAsync()
    {
        var brands = await _context.Brands
            .OrderBy(b => b.Name)
            .Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Origin = b.Origin,
                LogoUrl = b.LogoUrl
            })
            .ToListAsync();

        return brands;
    }

    private ProductListDto MapToProductListDto(Product product)
    {
        var variantWithStock = product.Variants
            .Where(v => v.IsActive)
            .Select(v => new
            {
                v.Price,
                v.OriginalPrice,
                Stock = v.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved)
            })
            .ToList();

        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Brand = product.Brand != null ? new BrandDto
            {
                Id = product.Brand.Id,
                Name = product.Brand.Name,
                Origin = product.Brand.Origin,
                LogoUrl = product.Brand.LogoUrl
            } : null,
            Category = product.Category != null ? new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Slug = product.Category.Slug
            } : null,
            ShortDescription = product.ShortDescription,
            PriceFrom = variantWithStock.Any() ? variantWithStock.Min(v => v.Price) : 0,
            PriceTo = variantWithStock.Any() ? variantWithStock.Max(v => v.Price) : 0,
            ThumbnailUrl = product.Variants.FirstOrDefault()?.ThumbnailUrl,
            IsFeatured = product.IsFeatured,
            DefaultVariantId = product.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault()?.Id
        };
    }

    private ProductDetailDto MapToProductDetailDto(Product product)
    {
        var detail = new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Specifications = product.Specifications,
            Brand = new BrandDto
            {
                Id = product.Brand!.Id,
                Name = product.Brand.Name,
                Origin = product.Brand.Origin,
                LogoUrl = product.Brand.LogoUrl
            },
            Category = new CategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name,
                Slug = product.Category.Slug
            },
            Variants = product.Variants
                .Where(v => v.IsActive)
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    OriginalPrice = v.OriginalPrice,
                    ThumbnailUrl = v.ThumbnailUrl,
                    QuantityAvailable = v.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved),
                    IsActive = v.IsActive
                }).ToList(),
            Images = product.Variants.SelectMany(v => v.Images)
                .OrderBy(img => img.SortOrder)
                .Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    AltText = img.AltText,
                    SortOrder = img.SortOrder
                }).ToList()
        };

        // RatingAverage và ReviewCount cần tính từ bảng reviews (chưa include)
        // Tạm thời để 0
        detail.RatingAverage = 0;
        detail.ReviewCount = 0;

        return detail;
    }
}
