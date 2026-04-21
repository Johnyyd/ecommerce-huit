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
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
    }

    public class AdjustStockRequest
    {
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

    /// <summary>
    /// Dashboard analytics for warehouse management
    /// </summary>
    public class WarehouseAnalyticsDto
    {
        public WarehouseAnalyticsDto()
        {
            WarehouseStats = new List<WarehouseStatsDto>();
            RecentTrends = new List<StockTrendDto>();
        }

        public int TotalWarehouses { get; set; }
        public int TotalSKUs { get; set; }
        public long TotalItemsInStock { get; set; }
        public long TotalItemsReserved { get; set; }
        public int LowStockItemsCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<WarehouseStatsDto> WarehouseStats { get; set; }
        public List<StockTrendDto> RecentTrends { get; set; }
    }

    public class WarehouseStatsDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseCode { get; set; }
        public long TotalItems { get; set; }
        public long AvailableItems { get; set; }
        public long ReservedItems { get; set; }
        public int SKUCount { get; set; }
        public int LowStockCount { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }

    public class StockTrendDto
    {
        public DateTime Date { get; set; }
        public string MovementType { get; set; }
        public long Quantity { get; set; }
        public int TransactionCount { get; set; }
    }

    public class InventoryReorderReportDto
    {
        public InventoryReorderReportDto()
        {
            StockByWarehouse = new List<WarehouseStockDto>();
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; }
        public int TotalQuantityAcrossWarehouses { get; set; }
        public int ReorderPoint { get; set; }
        public List<WarehouseStockDto> StockByWarehouse { get; set; }
        public string ReorderStatus { get; set; } // URGENT, WARNING, OK
    }

    public class WarehouseStockDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity { get; set; }
        public int Reserved { get; set; }
    }

    public class StockMovementFilterRequest
    {
        public StockMovementFilterRequest()
        {
            PageNumber = 1;
            PageSize = 50;
        }

        public int? WarehouseId { get; set; }
        public int? VariantId { get; set; }
        public string MovementType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
