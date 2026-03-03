using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities;

public class ProductSerial : BaseEntity
{
    public int VariantId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public SerialStatus Status { get; set; } = SerialStatus.AVAILABLE;
    public DateTime InboundDate { get; set; } = DateTime.UtcNow;
    public DateTime? OutboundDate { get; set; }
    public DateOnly? WarrantyExpireDate { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ProductVariant Variant { get; set; } = null!;
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual ICollection<OrderItemSerial> OrderItemSerials { get; set; } = new List<OrderItemSerial>();
}
