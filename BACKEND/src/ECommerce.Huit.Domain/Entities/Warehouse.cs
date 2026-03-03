using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public WarehouseType Type { get; set; } = WarehouseType.PHYSICAL;
    public string? Phone { get; set; }
    public string? Manager { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public virtual ICollection<ProductSerial> Serials { get; set; } = new List<ProductSerial>();
    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
