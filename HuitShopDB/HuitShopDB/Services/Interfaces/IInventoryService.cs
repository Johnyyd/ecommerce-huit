using System.Collections.Generic;
using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Admin;

namespace HuitShopDB.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetStockLevelByWarehouseAsync(int warehouseId);
        Task<IEnumerable<LowStockDto>> GetLowStockVariantsAsync(int? warehouseId);
        Task<bool> ImportStockAsync(ImportStockRequest request);
        Task<bool> TransferStockAsync(TransferStockRequest request);
        Task<bool> AdjustStockAsync(AdjustStockRequest request);
        Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(int warehouseId = 0, int? variantId = null);
        Task<IEnumerable<Models.warehouse>> GetWarehousesAsync();
        
        // New analytics methods
        Task<WarehouseAnalyticsDto> GetWarehouseAnalyticsAsync();
        Task<IEnumerable<InventoryReorderReportDto>> GetReorderReportAsync();
        Task<IEnumerable<StockMovementDto>> GetStockMovementsFilteredAsync(StockMovementFilterRequest filter);
    }
}

