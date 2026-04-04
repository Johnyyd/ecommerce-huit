using System.Web.Mvc;

namespace ECommerce.Huit.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Chào mừng bạn đến với ECommerce HUIT - Hệ thống quản lý bán hàng điện tử (VS 2013).";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Hệ thống được phát triển trên nền tảng ASP.NET MVC 5.";
            return View();
        }
    }
}
