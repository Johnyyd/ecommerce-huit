using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Product;

namespace HuitShopDB.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;

        public HomeController()
        {
            _productService = new Services.ProductService();
        }

        public ActionResult Index()
        {

            var categories = _productService.GetCategories();
            var query = new ProductQueryParams { PageSize = 10, SortBy = "newest" };
            var products = _productService.GetProducts(query);

            ViewBag.Categories = categories;
            ViewBag.FeaturedProducts = products;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Hệ thống được phát triển trên nền tảng ASP.NET MVC 5.";
            return View();
        }
    }
}


