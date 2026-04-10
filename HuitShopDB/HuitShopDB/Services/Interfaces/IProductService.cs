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
    }
}

