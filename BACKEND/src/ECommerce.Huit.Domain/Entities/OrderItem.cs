namespace ECommerce.Huit.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty; // snapshot
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual ProductVariant Variant { get; set; } = null!;
    public virtual ICollection<OrderItemSerial> OrderItemSerials { get; set; } = new List<OrderItemSerial>();
}
