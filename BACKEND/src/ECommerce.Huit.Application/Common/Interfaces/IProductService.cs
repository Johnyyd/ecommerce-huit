using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Huit.Application.DTOs.Product;

namespace ECommerce.Huit.Application.Common.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListDto>> GetProductsAsync(ProductQueryParams queryParams);
        Task<ProductDetailDto> GetProductDetailAsync(int productId);
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<IEnumerable<BrandDto>> GetBrandsAsync();
    }
}
