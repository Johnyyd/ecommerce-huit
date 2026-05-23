using System.Threading.Tasks;
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

        public async Task<ActionResult> Index()
        {
            var categories = await _productService.GetCategoriesAsync();
            var query = new ProductQueryParams { PageSize = 10, SortBy = "newest" };
            var products = await _productService.GetProductsAsync(query);

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

