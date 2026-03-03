using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int? BrandId { get; set; }
    public int CategoryId { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; } // HTML
    public string? Specifications { get; set; } // JSON string
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.DRAFT;
    public bool IsFeatured { get; set; } = false;
    public int CreatedBy { get; set; }

    // Navigation properties
    public virtual Brand? Brand { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
