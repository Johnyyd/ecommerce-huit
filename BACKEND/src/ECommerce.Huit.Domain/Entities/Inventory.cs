namespace ECommerce.Huit.Domain.Entities;

public class Inventory : BaseEntity
{
    public int WarehouseId { get; set; }
    public int VariantId { get; set; }
    public int QuantityOnHand { get; set; } = 0;
    public int QuantityReserved { get; set; } = 0;
    public int ReorderPoint { get; set; } = 10;

    // Navigation properties
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ProductVariant Variant { get; set; } = null!;
}
