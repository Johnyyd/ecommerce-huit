using System;
using System.Collections.Generic;

namespace HuitShopDB.Models.DTOs.Admin
{
    public class InventoryDto
    {
        public InventoryDto()
        {
            WarehouseName = string.Empty;
            WarehouseCode = string.Empty;
            Sku = string.Empty;
            ProductName = string.Empty;
        }

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public int VariantId { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class LowStockDto
    {
        public LowStockDto()
        {
            WarehouseName = string.Empty;
            WarehouseCode = string.Empty;
            ProductName = string.Empty;
            Sku = string.Empty;
        }

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int VariantId { get; set; }
        public string Sku { get; set; }
        public string VariantName { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class ImportStockRequest
    {
        public ImportStockRequest()
        {
            Serials = new List<string>();
        }

        public int WarehouseId { get; set; }
        public int VariantId { get; set; }
        public decimal CostPrice { get; set; }
        public int? SupplierId { get; set; }
        public List<string> Serials { get; set; }
    }

    public class TransferStockRequest
    {
        public TransferStockRequest()
        {
        }

        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
    }

    public class AdjustStockRequest
    {
        public AdjustStockRequest()
        {
        }

        public int WarehouseId { get; set; }
        public int VariantId { get; set; }
        public int QuantityChange { get; set; } // positive or negative
        public string Note { get; set; }
    }

    public class StockMovementDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } 
        public int VariantId { get; set; }
        public string Sku { get; set; } 
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public int Quantity { get; set; }
        public string MovementType { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

