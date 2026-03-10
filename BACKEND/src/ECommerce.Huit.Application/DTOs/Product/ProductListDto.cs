namespace ECommerce.Huit.Application.DTOs.Product;

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public BrandDto? Brand { get; set; }
    public CategoryDto? Category { get; set; }
    public string? ShortDescription { get; set; }
    public decimal PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal RatingAverage { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
    public int? DefaultVariantId { get; set; }
}
