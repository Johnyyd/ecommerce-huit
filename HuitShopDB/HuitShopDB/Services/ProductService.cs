using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Product;
using HuitShopDB.Models;

namespace HuitShopDB.Services
{
    public class ProductService : IProductService
    {
        private readonly HuitShopDBDataContext _context;

        public ProductService()
        {
            _context = new HuitShopDBDataContext();
        }

        public async Task<IEnumerable<ProductListDto>> GetProductsAsync(ProductQueryParams query)
        {
            var productsQuery = _context.products
                .Where(p => p.status == "ACTIVE")
                .AsQueryable();

            if (query.CategoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.category_id == query.CategoryId.Value);

            if (query.BrandId.HasValue)
                productsQuery = productsQuery.Where(p => p.brand_id == query.BrandId.Value);

            if (query.MinPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.product_variants.Any(v => v.price >= query.MinPrice.Value));

            if (query.MaxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.product_variants.Any(v => v.price <= query.MaxPrice.Value));

            if (!string.IsNullOrEmpty(query.Search))
                productsQuery = productsQuery.Where(p =>
                    p.name.Contains(query.Search) ||
                    (p.description != null && p.description.Contains(query.Search)));

            if (query.InStockOnly)
                productsQuery = productsQuery.Where(p =>
                    p.product_variants.Any(v => v.inventories.Any(i => i.quantity_on_hand - i.quantity_reserved > 0)));

            if (query.SortBy == "price_asc")
                productsQuery = productsQuery.OrderBy(p => p.product_variants.Min(v => (decimal?)v.price) ?? 0);
            else if (query.SortBy == "price_desc")
                productsQuery = productsQuery.OrderByDescending(p => p.product_variants.Max(v => (decimal?)v.price) ?? 0);
            else if (query.SortBy == "name")
                productsQuery = productsQuery.OrderBy(p => p.name);
            else
                productsQuery = productsQuery.OrderByDescending(p => p.created_at);

            var products = productsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var result = new List<ProductListDto>();
            foreach (var p in products)
            {
                result.Add(MapToProductListDto(p));
            }
            return await Task.FromResult(result);
        }

        public async Task<ProductDetailDto> GetProductDetailAsync(int productId)
        {
            var p = _context.products
                .FirstOrDefault(x => x.id == productId && x.status == "ACTIVE");

            if (p == null) return null;

            return await Task.FromResult(MapToProductDetailDto(p));
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = _context.categories
                .Where(c => c.is_active == true)
                .OrderBy(c => c.sort_order)
                .ToList();

            var categoryDict = new Dictionary<int, CategoryDto>();
            foreach (var c in categories)
            {
                var dto = new CategoryDto();
                dto.Id = c.id;
                dto.Name = c.name;
                dto.Slug = c.slug;
                dto.Description = c.description;
                dto.ParentId = c.parent_id;
                dto.Children = new List<CategoryDto>();
                categoryDict.Add(c.id, dto);
            }

            var result = new List<CategoryDto>();
            foreach (var category in categories)
            {
                if (category.parent_id.HasValue && categoryDict.ContainsKey(category.parent_id.Value))
                {
                    categoryDict[category.parent_id.Value].Children.Add(categoryDict[category.id]);
                }
                else if (!category.parent_id.HasValue)
                {
                    result.Add(categoryDict[category.id]);
                }
            }

            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<BrandDto>> GetBrandsAsync()
        {
            var brands = _context.brands
                .OrderBy(b => b.name)
                .ToList();

            var result = new List<BrandDto>();
            foreach (var b in brands)
            {
                var dto = new BrandDto();
                dto.Id = b.id;
                dto.Name = b.name;
                dto.Origin = b.origin;
                dto.LogoUrl = b.logo_url;
                result.Add(dto);
            }

            return await Task.FromResult(result);
        }

        private ProductListDto MapToProductListDto(product p)
        {
            var dto = new ProductListDto();
            dto.Id = p.id;
            dto.Name = p.name;
            dto.Slug = p.slug;
            dto.ShortDescription = p.short_description;
            dto.IsFeatured = p.is_featured;

            if (p.brand != null)
            {
                var brandDto = new BrandDto();
                brandDto.Id = p.brand.id;
                brandDto.Name = p.brand.name;
                brandDto.Origin = p.brand.origin;
                brandDto.LogoUrl = p.brand.logo_url;
                dto.Brand = brandDto;
            }

            if (p.category != null)
            {
                var catDto = new CategoryDto();
                catDto.Id = p.category.id;
                catDto.Name = p.category.name;
                catDto.Slug = p.category.slug;
                dto.Category = catDto;
            }

            var variants = p.product_variants.Where(v => v.is_active == true).ToList();
            if (variants.Any())
            {
                dto.PriceFrom = variants.Min(v => v.price);
                dto.PriceTo = variants.Max(v => v.price);
                var firstVariant = variants.FirstOrDefault();
                dto.ThumbnailUrl = firstVariant != null ? firstVariant.thumbnail_url : null;
                var displayVariant = variants.OrderBy(v => v.display_order).FirstOrDefault();
                dto.DefaultVariantId = displayVariant != null ? displayVariant.id : 0;
            }

            dto.RatingAverage = 0;
            dto.ReviewCount = 0;

            return dto;
        }

        private ProductDetailDto MapToProductDetailDto(product p)
        {
            var detail = new ProductDetailDto();
            detail.Id = p.id;
            detail.Name = p.name;
            detail.Slug = p.slug;
            detail.Description = p.description;
            detail.Specifications = p.specifications;

            if (p.brand != null)
            {
                var brandDto = new BrandDto();
                brandDto.Id = p.brand.id;
                brandDto.Name = p.brand.name;
                brandDto.Origin = p.brand.origin;
                brandDto.LogoUrl = p.brand.logo_url;
                detail.Brand = brandDto;
            }

            if (p.category != null)
            {
                var catDto = new CategoryDto();
                catDto.Id = p.category.id;
                catDto.Name = p.category.name;
                catDto.Slug = p.category.slug;
                detail.Category = catDto;
            }

            detail.Variants = new List<ProductVariantDto>();
            foreach (var v in p.product_variants.Where(v1 => v1.is_active == true))
            {
                var vDto = new ProductVariantDto();
                vDto.Id = v.id;
                vDto.Sku = v.sku;
                vDto.VariantName = v.variant_name;
                vDto.Price = v.price;
                vDto.OriginalPrice = v.original_price;
                vDto.ThumbnailUrl = v.thumbnail_url;
                vDto.QuantityAvailable = v.inventories.Sum(i => i.quantity_on_hand - i.quantity_reserved);
                vDto.IsActive = v.is_active;
                detail.Variants.Add(vDto);
            }

            detail.Images = new List<ProductImageDto>();
            var allImages = p.product_variants.SelectMany(v => v.product_images).OrderBy(img => img.sort_order).ToList();
            foreach (var img in allImages)
            {
                var imgDto = new ProductImageDto();
                imgDto.Id = img.id;
                imgDto.ImageUrl = img.image_url;
                imgDto.AltText = img.alt_text;
                imgDto.SortOrder = img.sort_order;
                detail.Images.Add(imgDto);
            }

            detail.RatingAverage = 0;
            detail.ReviewCount = 0;

            return detail;
        }
    }
}

