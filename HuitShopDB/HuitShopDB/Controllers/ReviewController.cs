using System.Web.Mvc;

namespace HuitShopDB.Controllers
{
    public class ReviewController : Controller
    {
        // GET: /Review/
        public ActionResult Index()
        {
            ViewBag.Title = "Quản lý đánh giá và phản hồi";
            return View();
        }
    }
}
