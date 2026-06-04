using System.Collections.Generic;
using HuitShopDB.Models.DTOs.Product;

namespace HuitShopDB.Services.Interfaces
{
    public interface IProductService
    {
        IEnumerable<ProductListDto> GetProducts(ProductQueryParams queryParams);
        int GetProductsCount(ProductQueryParams queryParams);
        ProductDetailDto GetProductDetail(int productId);
        IEnumerable<CategoryDto> GetCategories();
        IEnumerable<BrandDto> GetBrands();

        // Admin Management Methods
        IEnumerable<ProductListDto> GetAdminProducts(string search, int? categoryId, string status, int page, int pageSize);
        int GetAdminProductsCount(string search, int? categoryId, string status);
        ProductDetailDto GetAdminProductDetail(int productId);
        int CreateProduct(ProductCreateDto dto);
        bool UpdateProduct(int id, ProductEditDto dto);
        bool ToggleProductStatus(int id, string status);
        bool CreateVariant(int productId, VariantCreateDto dto);
        bool UpdateVariant(int variantId, VariantEditDto dto);
        bool AddProductImage(int variantId, string imageUrl, string altText, int sortOrder);
        bool DeleteProductImage(int imageId);
    }
}


