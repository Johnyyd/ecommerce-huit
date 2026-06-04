using System.Collections.Generic;
using HuitShopDB.Models.DTOs.Admin;

namespace HuitShopDB.Services.Interfaces
{
    public interface IInventoryService
    {
        IEnumerable<InventoryDto> GetStockLevelByWarehouse(int warehouseId);
        IEnumerable<LowStockDto> GetLowStockVariants(int? warehouseId);
        bool ImportStock(ImportStockRequest request);
        bool TransferStock(TransferStockRequest request);
        bool AdjustStock(AdjustStockRequest request);
        IEnumerable<StockMovementDto> GetStockMovements(int warehouseId = 0, int? variantId = null);
        IEnumerable<Models.warehouse> GetWarehouses();
        IEnumerable<Models.product_variant> GetProductVariants();
        
        // New analytics methods
        WarehouseAnalyticsDto GetWarehouseAnalytics();
        IEnumerable<InventoryReorderReportDto> GetReorderReport();
        IEnumerable<StockMovementDto> GetStockMovementsFiltered(StockMovementFilterRequest filter);
    }
}



