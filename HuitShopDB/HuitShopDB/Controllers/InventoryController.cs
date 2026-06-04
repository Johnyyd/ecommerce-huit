using System;
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

        private bool IsAdminOrStaff()
        {
            string role = Session["UserRole"] as string;
            return role == "ADMIN" || role == "STAFF";
        }

        // GET: /Inventory/
        public ActionResult Index(int warehouseId = 0, bool lowStock = false)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Title = "Quản lý kho hàng";
            
            var warehouses = _inventoryService.GetWarehouses();
            ViewBag.Warehouses = warehouses;
            ViewBag.CurrentWarehouseId = warehouseId;
            ViewBag.LowStockOnly = lowStock;

            if (lowStock)
            {
                var lowStockList = _inventoryService.GetLowStockVariants(warehouseId == 0 ? (int?)null : warehouseId);
                return View("LowStock", lowStockList);
            }

            var inventory = _inventoryService.GetStockLevelByWarehouse(warehouseId);
            return View(inventory);
        }

        // GET: /Inventory/Dashboard
        public ActionResult Dashboard()
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Title = "Dashboard - Quản lý kho";
            var analytics = _inventoryService.GetWarehouseAnalytics();
            var reorderReport = _inventoryService.GetReorderReport();
            
            ViewBag.ReorderReport = reorderReport;
            return View(analytics);
        }

        // POST: /Inventory/Adjust
        [HttpPost]
        public ActionResult Adjust(AdjustStockRequest request)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                bool success = _inventoryService.AdjustStock(request);
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
        public ActionResult Import(int warehouseId = 0, int variantId = 0)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Warehouses = _inventoryService.GetWarehouses();
            ViewBag.Variants = _inventoryService.GetProductVariants();
            
            var request = new ImportStockRequest
            {
                WarehouseId = warehouseId,
                VariantId = variantId
            };
            return View(request);
        }

        // POST: /Inventory/Import
        [HttpPost]
        public ActionResult Import(ImportStockRequest request, string serialsRaw)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            if (!string.IsNullOrEmpty(serialsRaw))
            {
                request.Serials = new System.Collections.Generic.List<string>(
                    serialsRaw.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)
                );
            }

            if (ModelState.IsValid)
            {
                bool success = _inventoryService.ImportStock(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Nhập kho thành công.";
                    return RedirectToAction("Index", new { warehouseId = request.WarehouseId });
                }
            }
            ViewBag.Warehouses = _inventoryService.GetWarehouses();
            ViewBag.Variants = _inventoryService.GetProductVariants();
            return View(request);
        }

        // GET: /Inventory/Transfer
        public ActionResult Transfer(int warehouseId = 0, int variantId = 0)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Title = "Chuyển kho";
            ViewBag.Warehouses = _inventoryService.GetWarehouses();
            ViewBag.Variants = _inventoryService.GetProductVariants();
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
        public ActionResult Transfer(TransferStockRequest request)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                bool success = _inventoryService.TransferStock(request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Chuyển kho thành công.";
                    return RedirectToAction("Index", new { warehouseId = request.FromWarehouseId });
                }
                TempData["ErrorMessage"] = "Chuyển kho thất bại. Vui lòng kiểm tra lại số lượng và kho đích.";
            }

            ViewBag.Warehouses = _inventoryService.GetWarehouses();
            ViewBag.Variants = _inventoryService.GetProductVariants();
            return View(request);
        }

        // GET: /Inventory/History
        public ActionResult History(int warehouseId = 0, int? variantId = null)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            if (variantId == 0) variantId = null;

            ViewBag.Title = "Lịch sử tồn kho";
            ViewBag.Warehouses = _inventoryService.GetWarehouses();
            ViewBag.Variants = _inventoryService.GetProductVariants();
            ViewBag.CurrentWarehouseId = warehouseId;
            ViewBag.CurrentVariantId = variantId;

            var movements = _inventoryService.GetStockMovements(warehouseId, variantId);
            return View(movements);
        }

        // GET: /Inventory/ReorderReport
        public ActionResult ReorderReport()
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Title = "Báo cáo hàng cần đặt lại";
            var report = _inventoryService.GetReorderReport();
            return View(report);
        }
    }
}

