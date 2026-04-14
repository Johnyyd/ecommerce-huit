using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Admin;

namespace HuitShopDB.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController()
        {
            _inventoryService = new Services.InventoryService();
        }

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET: /Inventory/
        public async Task<ActionResult> Index(int warehouseId = 0, bool lowStock = false)
        {
            ViewBag.Title = "Quản lý kho hàng";
            
            var warehouses = await _inventoryService.GetWarehousesAsync();
            ViewBag.Warehouses = warehouses;
            ViewBag.CurrentWarehouseId = warehouseId;
            ViewBag.LowStockOnly = lowStock;

            if (lowStock)
            {
                var lowStockList = await _inventoryService.GetLowStockVariantsAsync(warehouseId == 0 ? (int?)null : warehouseId);
                return View("LowStock", lowStockList);
            }

            var inventory = await _inventoryService.GetStockLevelByWarehouseAsync(warehouseId);
            return View(inventory);
        }

        // POST: /Inventory/Adjust
        [HttpPost]
        public async Task<ActionResult> Adjust(AdjustStockRequest request)
        {
            if (ModelState.IsValid)
            {
                bool success = await _inventoryService.AdjustStockAsync(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Điều chỉnh kho thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể điều chỉnh kho. Vui lòng kiểm tra lại dữ liệu.";
                }
            }
            return RedirectToAction("Index", new { warehouseId = request.WarehouseId });
        }

        // GET: /Inventory/Import
        public async Task<ActionResult> Import()
        {
            ViewBag.Warehouses = await _inventoryService.GetWarehousesAsync();
            return View(new ImportStockRequest());
        }

        // POST: /Inventory/Import
        [HttpPost]
        public async Task<ActionResult> Import(ImportStockRequest request, string serialsRaw)
        {
            if (!string.IsNullOrEmpty(serialsRaw))
            {
                request.Serials = new System.Collections.Generic.List<string>(
                    serialsRaw.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                );
            }

            if (ModelState.IsValid)
            {
                bool success = await _inventoryService.ImportStockAsync(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Nhập kho thành công.";
                    return RedirectToAction("Index", new { warehouseId = request.WarehouseId });
                }
            }
            ViewBag.Warehouses = await _inventoryService.GetWarehousesAsync();
            return View(request);
        }
    }
}
