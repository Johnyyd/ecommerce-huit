namespace ECommerce.Huit.Application.DTOs.Product;

public class ProductQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "newest"; // "price_asc", "price_desc", "newest", "name"
    public bool InStockOnly { get; set; } = false;
}
