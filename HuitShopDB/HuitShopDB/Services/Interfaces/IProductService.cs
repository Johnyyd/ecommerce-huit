using System.Collections.Generic;
using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Product;

namespace HuitShopDB.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListDto>> GetProductsAsync(ProductQueryParams queryParams);
        Task<ProductDetailDto> GetProductDetailAsync(int productId);
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<BrandDto>> GetBrandsAsync();

        // Admin Management Methods
        Task<IEnumerable<ProductListDto>> GetAdminProductsAsync(string search, int? categoryId, string status, int page, int pageSize);
        Task<int> GetAdminProductsCountAsync(string search, int? categoryId, string status);
        Task<ProductDetailDto> GetAdminProductDetailAsync(int productId);
        Task<int> CreateProductAsync(ProductCreateDto dto);
        Task<bool> UpdateProductAsync(int id, ProductEditDto dto);
        Task<bool> ToggleProductStatusAsync(int id, string status);
        Task<bool> CreateVariantAsync(int productId, VariantCreateDto dto);
        Task<bool> UpdateVariantAsync(int variantId, VariantEditDto dto);
        Task<bool> AddProductImageAsync(int variantId, string imageUrl, string altText, int sortOrder);
        Task<bool> DeleteProductImageAsync(int imageId);
    }
}

