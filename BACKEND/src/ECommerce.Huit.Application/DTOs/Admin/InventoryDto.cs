namespace ECommerce.Huit.Application.DTOs.Admin;

public class InventoryDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int VariantId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReorderPoint { get; set; }
}

public class LowStockDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VariantId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReorderPoint { get; set; }
}

public class ImportStockRequest
{
    public int WarehouseId { get; set; }
    public int VariantId { get; set; }
    public decimal CostPrice { get; set; }
    public int? SupplierId { get; set; }
    public List<string> Serials { get; set; } = new();
}

public class TransferStockRequest
{
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int VariantId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class AdjustStockRequest
{
    public int WarehouseId { get; set; }
    public int VariantId { get; set; }
    public int QuantityChange { get; set; } // positive or negative
    public string? Note { get; set; }
}
