namespace ECommerce.Huit.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int? WeightGrams { get; set; }
    public string? Dimensions { get; set; } // JSON: {"length":...,"width":...,"height":...}

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public virtual ICollection<ProductSerial> Serials { get; set; } = new List<ProductSerial>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
