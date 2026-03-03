namespace ECommerce.Huit.Application.DTOs.Product;

public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Specifications { get; set; } // JSON
    public BrandDto Brand { get; set; } = null!;
    public CategoryDto Category { get; set; } = null!;
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ReviewDto> Reviews { get; set; } = new();
    public decimal RatingAverage { get; set; }
    public int ReviewCount { get; set; }
}

public class ProductVariantDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int QuantityAvailable { get; set; }
    public bool IsActive { get; set; }
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public UserSimpleDto User { get; set; } = null!;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserSimpleDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
