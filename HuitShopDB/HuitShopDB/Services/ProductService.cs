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
            dto.Status = p.status;
            dto.IsActive = (p.status == "ACTIVE");

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
            detail.Status = p.status;
            detail.IsFeatured = p.is_featured;

            return detail;
        }

        public async Task<IEnumerable<ProductListDto>> GetAdminProductsAsync(string search, int? categoryId, string status, int page, int pageSize)
        {
            var query = _context.products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.name.Contains(search) || (p.description != null && p.description.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.category_id == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(status) && status != "ALL")
            {
                query = query.Where(p => p.status == status);
            }

            var products = query
                .OrderByDescending(p => p.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new List<ProductListDto>();
            foreach (var p in products)
            {
                result.Add(MapToProductListDto(p));
            }
            return await Task.FromResult(result);
        }

        public async Task<int> GetAdminProductsCountAsync(string search, int? categoryId, string status)
        {
            var query = _context.products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.name.Contains(search) || (p.description != null && p.description.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.category_id == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(status) && status != "ALL")
            {
                query = query.Where(p => p.status == status);
            }

            return await Task.FromResult(query.Count());
        }

        public async Task<ProductDetailDto> GetAdminProductDetailAsync(int productId)
        {
            var p = _context.products.FirstOrDefault(x => x.id == productId);
            if (p == null) return null;

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
            // Load ALL variants including inactive ones for Admin editing
            foreach (var v in p.product_variants)
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

            return await Task.FromResult(detail);
        }

        public async Task<int> CreateProductAsync(ProductCreateDto dto)
        {
            try
            {
                var p = new product
                {
                    name = dto.Name,
                    slug = ToFriendlySlug(dto.Name),
                    category_id = dto.CategoryId,
                    brand_id = dto.BrandId,
                    short_description = dto.ShortDescription,
                    description = dto.Description,
                    specifications = dto.Specifications,
                    status = dto.Status ?? "DRAFT",
                    is_featured = dto.IsFeatured,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.products.InsertOnSubmit(p);
                _context.SubmitChanges();

                // Insert the first default variant
                var variant = new product_variant
                {
                    product_id = p.id,
                    variant_name = string.IsNullOrEmpty(dto.DefaultVariantName) ? "Bản tiêu chuẩn" : dto.DefaultVariantName,
                    sku = string.IsNullOrEmpty(dto.DefaultSku) ? "SKU-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper() : dto.DefaultSku,
                    price = dto.DefaultPrice,
                    original_price = dto.DefaultOriginalPrice,
                    thumbnail_url = dto.DefaultThumbnailUrl,
                    is_active = true,
                    display_order = 1,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.product_variants.InsertOnSubmit(variant);
                _context.SubmitChanges();

                return await Task.FromResult(p.id);
            }
            catch (Exception)
            {
                return await Task.FromResult(0);
            }
        }

        public async Task<bool> UpdateProductAsync(int id, ProductEditDto dto)
        {
            try
            {
                var p = _context.products.FirstOrDefault(x => x.id == id);
                if (p == null) return await Task.FromResult(false);

                p.name = dto.Name;
                p.slug = ToFriendlySlug(dto.Name);
                p.category_id = dto.CategoryId;
                p.brand_id = dto.BrandId;
                p.short_description = dto.ShortDescription;
                p.description = dto.Description;
                p.specifications = dto.Specifications;
                p.status = dto.Status;
                p.is_featured = dto.IsFeatured;
                p.updated_at = DateTime.Now;

                _context.SubmitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> ToggleProductStatusAsync(int id, string status)
        {
            try
            {
                var p = _context.products.FirstOrDefault(x => x.id == id);
                if (p == null) return await Task.FromResult(false);

                p.status = status;
                p.updated_at = DateTime.Now;

                _context.SubmitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> CreateVariantAsync(int productId, VariantCreateDto dto)
        {
            try
            {
                var variant = new product_variant
                {
                    product_id = productId,
                    variant_name = dto.VariantName,
                    sku = dto.Sku,
                    price = dto.Price,
                    original_price = dto.OriginalPrice,
                    thumbnail_url = dto.ThumbnailUrl,
                    is_active = dto.IsActive,
                    display_order = dto.DisplayOrder,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                _context.product_variants.InsertOnSubmit(variant);
                _context.SubmitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> UpdateVariantAsync(int variantId, VariantEditDto dto)
        {
            try
            {
                var v = _context.product_variants.FirstOrDefault(x => x.id == variantId);
                if (v == null) return await Task.FromResult(false);

                v.variant_name = dto.VariantName;
                v.sku = dto.Sku;
                v.price = dto.Price;
                v.original_price = dto.OriginalPrice;
                if (!string.IsNullOrEmpty(dto.ThumbnailUrl))
                {
                    v.thumbnail_url = dto.ThumbnailUrl;
                }
                v.is_active = dto.IsActive;
                v.display_order = dto.DisplayOrder;
                v.updated_at = DateTime.Now;

                _context.SubmitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> AddProductImageAsync(int variantId, string imageUrl, string altText, int sortOrder)
        {
            try
            {
                var img = new product_image
                {
                    variant_id = variantId,
                    image_url = imageUrl,
                    alt_text = altText,
                    sort_order = sortOrder,
                    created_at = DateTime.Now
                };

                _context.product_images.InsertOnSubmit(img);
                _context.SubmitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeleteProductImageAsync(int imageId)
        {
            try
            {
                var img = _context.product_images.FirstOrDefault(x => x.id == imageId);
                if (img == null) return await Task.FromResult(false);

                _context.product_images.DeleteOnSubmit(img);
                _context.SubmitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return await Task.FromResult(false);
            }
        }

        public static string ToFriendlySlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";
            title = title.ToLowerInvariant().Trim();

            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ",
                "í", "ì", "ỉ", "ĩ", "ị",
                "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ",
                "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự",
                "ý", "ỳ", "ỷ", "ỹ", "ỵ" };
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
                "i", "i", "i", "i", "i",
                "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
                "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u",
                "y", "y", "y", "y", "y" };
            for (int i = 0; i < arr1.Length; i++)
            {
                title = title.Replace(arr1[i], arr2[i]);
            }

            title = System.Text.RegularExpressions.Regex.Replace(title, @"[^a-z0-9\s-]", "");
            title = System.Text.RegularExpressions.Regex.Replace(title, @"\s+", "-");
            title = System.Text.RegularExpressions.Regex.Replace(title, @"-+", "-");
            return title.Trim('-');
        }
    }
}

