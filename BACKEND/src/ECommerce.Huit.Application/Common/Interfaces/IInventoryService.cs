using ECommerce.Huit.Application.DTOs.Admin;

namespace ECommerce.Huit.Application.Common.Interfaces;

public interface IInventoryService
{
    Task<InventoryResponse> ImportStockAsync(ImportStockRequest request, int staffId);
    Task<IEnumerable<InventoryDto>> GetInventoryAsync(int? warehouseId = null);
    Task<IEnumerable<LowStockDto>> GetLowStockReportAsync(int? warehouseId);
    Task<bool> TransferStockAsync(TransferStockRequest request, int staffId);
    Task<bool> AdjustStockAsync(AdjustStockRequest request, int staffId);
}
