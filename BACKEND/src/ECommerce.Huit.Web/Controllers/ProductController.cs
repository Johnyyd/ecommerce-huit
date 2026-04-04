using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using ECommerce.Huit.Application.Common.Interfaces;
using ECommerce.Huit.Application.DTOs.Product;

namespace ECommerce.Huit.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<ActionResult> Index(int? categoryId, string search, string sortBy, int page = 1)
        {
            var query = new ProductQueryParams();
            query.CategoryId = categoryId;
            query.Search = search;
            query.SortBy = sortBy ?? "newest";
            query.Page = page;
            query.PageSize = 12;

            var products = await _productService.GetProductsAsync(query);
            var categories = await _productService.GetCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = query.SortBy;

            return View(products);
        }

        public async Task<ActionResult> Detail(int id)
        {
            var product = await _productService.GetProductDetailAsync(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }
    }
}
