using System.Web.Mvc;

namespace HuitShopDB.Controllers
{
    public class WarrantyController : Controller
    {
        // GET: /Warranty/
        public ActionResult Index()
        {
            ViewBag.Title = "Quản lý bảo hành";
            return View();
        }
    }
}
