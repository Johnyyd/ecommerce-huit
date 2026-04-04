using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Product;
using ECommerce.Huit.Domain.Entities;

namespace ECommerce.Huit.Application.Services
{
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
                .Include(p => p.Variants.Select(v => v.Images))
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
            if (query.SortBy == "price_asc")
                productsQuery = productsQuery.OrderBy(p => p.Variants.Min(v => v.Price));
            else if (query.SortBy == "price_desc")
                productsQuery = productsQuery.OrderByDescending(p => p.Variants.Max(v => v.Price));
            else if (query.SortBy == "name")
                productsQuery = productsQuery.OrderBy(p => p.Name);
            else
                productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);

            // Pagination
            var products = await productsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            // Map to DTO
            var result = new List<ProductListDto>();
            foreach (var p in products)
            {
                result.Add(MapToProductListDto(p));
            }
            return result;
        }

        public async Task<ProductDetailDto> GetProductDetailAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Variants.Select(v => v.Images))
                .Include(p => p.Variants.Select(v => v.Inventories))
                .FirstOrDefaultAsync(p => p.Id == productId && p.Status == Domain.Enums.ProductStatus.ACTIVE);

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
            var categoryDict = new Dictionary<int, CategoryDto>();
            foreach (var c in categories)
            {
                var dto = new CategoryDto();
                dto.Id = c.Id;
                dto.Name = c.Name;
                dto.Slug = c.Slug;
                dto.Description = c.Description;
                dto.ParentId = c.ParentId;
                dto.Children = new List<CategoryDto>();
                categoryDict.Add(c.Id, dto);
            }

            var result = new List<CategoryDto>();
            foreach (var category in categories)
            {
                if (category.ParentId.HasValue && categoryDict.ContainsKey(category.ParentId.Value))
                {
                    categoryDict[category.ParentId.Value].Children.Add(categoryDict[category.Id]);
                }
                else if (!category.ParentId.HasValue)
                {
                    result.Add(categoryDict[category.Id]);
                }
            }

            return result;
        }

        public async Task<IEnumerable<BrandDto>> GetBrandsAsync()
        {
            var brands = await _context.Brands
                .OrderBy(b => b.Name)
                .ToListAsync();

            var result = new List<BrandDto>();
            foreach (var b in brands)
            {
                var dto = new BrandDto();
                dto.Id = b.Id;
                dto.Name = b.Name;
                dto.Origin = b.Origin;
                dto.LogoUrl = b.LogoUrl;
                result.Add(dto);
            }

            return result;
        }

        private ProductListDto MapToProductListDto(Product product)
        {
            var dto = new ProductListDto();
            dto.Id = product.Id;
            dto.Name = product.Name;
            dto.Slug = product.Slug;
            dto.ShortDescription = product.ShortDescription;
            dto.IsFeatured = product.IsFeatured;

            if (product.Brand != null)
            {
                var brandDto = new BrandDto();
                brandDto.Id = product.Brand.Id;
                brandDto.Name = product.Brand.Name;
                brandDto.Origin = product.Brand.Origin;
                brandDto.LogoUrl = product.Brand.LogoUrl;
                dto.Brand = brandDto;
            }

            if (product.Category != null)
            {
                var catDto = new CategoryDto();
                catDto.Id = product.Category.Id;
                catDto.Name = product.Category.Name;
                catDto.Slug = product.Category.Slug;
                dto.Category = catDto;
            }

            var variants = product.Variants.Where(v => v.IsActive).ToList();
            if (variants.Any())
            {
                dto.PriceFrom = variants.Min(v => v.Price);
                dto.PriceTo = variants.Max(v => v.Price);
                dto.ThumbnailUrl = variants.FirstOrDefault().ThumbnailUrl;
                dto.DefaultVariantId = variants.OrderBy(v => v.DisplayOrder).FirstOrDefault().Id;
            }

            dto.RatingAverage = 0;
            dto.ReviewCount = 0;

            return dto;
        }

        private ProductDetailDto MapToProductDetailDto(Product product)
        {
            var detail = new ProductDetailDto();
            detail.Id = product.Id;
            detail.Name = product.Name;
            detail.Slug = product.Slug;
            detail.Description = product.Description;
            detail.Specifications = product.Specifications;

            var brandDto = new BrandDto();
            brandDto.Id = product.Brand.Id;
            brandDto.Name = product.Brand.Name;
            brandDto.Origin = product.Brand.Origin;
            brandDto.LogoUrl = product.Brand.LogoUrl;
            detail.Brand = brandDto;

            var catDto = new CategoryDto();
            catDto.Id = product.Category.Id;
            catDto.Name = product.Category.Name;
            catDto.Slug = product.Category.Slug;
            detail.Category = catDto;

            detail.Variants = new List<ProductVariantDto>();
            foreach (var v in product.Variants.Where(v1 => v1.IsActive))
            {
                var vDto = new ProductVariantDto();
                vDto.Id = v.Id;
                vDto.Sku = v.Sku;
                vDto.VariantName = v.VariantName;
                vDto.Price = v.Price;
                vDto.OriginalPrice = v.OriginalPrice;
                vDto.ThumbnailUrl = v.ThumbnailUrl;
                vDto.QuantityAvailable = v.Inventories.Sum(i => i.QuantityOnHand - i.QuantityReserved);
                vDto.IsActive = v.IsActive;
                detail.Variants.Add(vDto);
            }

            detail.Images = new List<ProductImageDto>();
            var allImages = product.Variants.SelectMany(v => v.Images).OrderBy(img => img.SortOrder).ToList();
            foreach (var img in allImages)
            {
                var imgDto = new ProductImageDto();
                imgDto.Id = img.Id;
                imgDto.ImageUrl = img.ImageUrl;
                imgDto.AltText = img.AltText;
                imgDto.SortOrder = img.SortOrder;
                detail.Images.Add(imgDto);
            }

            detail.RatingAverage = 0;
            detail.ReviewCount = 0;

            return detail;
        }
    }
}
