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

        // GET: /Inventory/Dashboard
        public async Task<ActionResult> Dashboard()
        {
            ViewBag.Title = "Dashboard - Quản lý kho";
            var analytics = await _inventoryService.GetWarehouseAnalyticsAsync();
            var reorderReport = await _inventoryService.GetReorderReportAsync();
            
            ViewBag.ReorderReport = reorderReport;
            return View(analytics);
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

        // GET: /Inventory/Transfer
        public async Task<ActionResult> Transfer(int warehouseId = 0, int variantId = 0)
        {
            ViewBag.Title = "Chuyển kho";
            ViewBag.Warehouses = await _inventoryService.GetWarehousesAsync();
            var model = new TransferStockRequest
            {
                FromWarehouseId = warehouseId,
                VariantId = variantId,
                Quantity = 1
            };
            return View(model);
        }

        // POST: /Inventory/Transfer
        [HttpPost]
        public async Task<ActionResult> Transfer(TransferStockRequest request)
        {
            if (ModelState.IsValid)
            {
                bool success = await _inventoryService.TransferStockAsync(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Chuyển kho thành công.";
                    return RedirectToAction("Index", new { warehouseId = request.FromWarehouseId });
                }
                TempData["ErrorMessage"] = "Chuyển kho thất bại. Vui lòng kiểm tra lại số lượng và kho đích.";
            }

            ViewBag.Warehouses = await _inventoryService.GetWarehousesAsync();
            return View(request);
        }

        // GET: /Inventory/History
        public async Task<ActionResult> History(int warehouseId = 0, int? variantId = null)
        {
            ViewBag.Title = "Lịch sử tồn kho";
            ViewBag.Warehouses = await _inventoryService.GetWarehousesAsync();
            ViewBag.CurrentWarehouseId = warehouseId;
            ViewBag.CurrentVariantId = variantId;

            var movements = await _inventoryService.GetStockMovementsAsync(warehouseId, variantId);
            return View(movements);
        }

        // GET: /Inventory/ReorderReport
        public async Task<ActionResult> ReorderReport()
        {
            ViewBag.Title = "Báo cáo hàng cần đặt lại";
            var report = await _inventoryService.GetReorderReportAsync();
            return View(report);
        }
    }
}
