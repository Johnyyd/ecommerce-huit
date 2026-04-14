using System.Web.Mvc;

namespace HuitShopDB.Controllers
{
    public class UserController : Controller
    {
        // GET: /User/
        public ActionResult Index()
        {
            ViewBag.Title = "Quản lý người dùng";
            return View();
        }
    }
}
