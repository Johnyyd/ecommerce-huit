namespace ECommerce.Huit.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int VariantId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; } = 0;

    public virtual ProductVariant Variant { get; set; } = null!;
}
