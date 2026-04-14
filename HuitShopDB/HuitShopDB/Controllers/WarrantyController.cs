using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;

namespace HuitShopDB.Controllers
{
    public class WarrantyController : Controller
    {
        private readonly IWarrantyService _warrantyService;

        public WarrantyController()
        {
            _warrantyService = new Services.WarrantyService();
        }

        public WarrantyController(IWarrantyService warrantyService)
        {
            _warrantyService = warrantyService;
        }

        // GET: /Warranty/
        public async Task<ActionResult> Index(string serialNumber)
        {
            ViewBag.Title = "Tra cứu bảo hành";
            
            if (!string.IsNullOrEmpty(serialNumber))
            {
                var result = await _warrantyService.GetWarrantyBySerialAsync(serialNumber);
                if (result != null)
                {
                    return RedirectToAction("Details", new { serial = result.SerialNumber });
                }
                else
                {
                    ViewBag.ErrorMessage = "Không tìm thấy mã Serial này trong hệ thống.";
                }
            }

            var recent = await _warrantyService.GetRecentWarrantiesAsync(10);
            return View(recent);
        }

        // GET: /Warranty/Details?serial=...
        public async Task<ActionResult> Details(string serial)
        {
            if (string.IsNullOrEmpty(serial)) return RedirectToAction("Index");

            var result = await _warrantyService.GetWarrantyBySerialAsync(serial);
            if (result == null) return HttpNotFound();

            ViewBag.Title = "Chứng nhận bảo hành";
            return View(result);
        }
    }
}
