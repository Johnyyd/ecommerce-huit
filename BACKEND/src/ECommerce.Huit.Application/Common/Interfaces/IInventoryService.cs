using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Huit.Application.DTOs.Admin;

namespace ECommerce.Huit.Application.Common.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetStockLevelByWarehouseAsync(int warehouseId);
        Task<IEnumerable<LowStockDto>> GetLowStockVariantsAsync(int? warehouseId);
        Task<bool> ImportStockAsync(ImportStockRequest request);
        Task<bool> TransferStockAsync(TransferStockRequest request);
        Task<bool> AdjustStockAsync(AdjustStockRequest request);
    }
}
