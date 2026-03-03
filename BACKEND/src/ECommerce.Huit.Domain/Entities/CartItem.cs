namespace ECommerce.Huit.Domain.Entities;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public int VariantId { get; set; }
    public int Quantity { get; set; }

    // Navigation properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual ProductVariant Variant { get; set; } = null!;
}
