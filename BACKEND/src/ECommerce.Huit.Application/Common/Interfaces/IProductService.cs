using ECommerce.Huit.Application.DTOs.Product;

namespace ECommerce.Huit.Application.Common.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductListDto>> GetProductsAsync(ProductQueryParams query);
    Task<ProductDetailDto?> GetProductByIdAsync(int id);
    Task<ProductDetailDto?> GetProductBySlugAsync(string slug);
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<IEnumerable<BrandDto>> GetBrandsAsync();
}
