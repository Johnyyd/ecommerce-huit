using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Product;

namespace HuitShopDB.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;

        public ProductController()
        {
            _productService = new Services.ProductService();
            _reviewService = new Services.ReviewService();
        }

        public ProductController(IProductService productService, IReviewService reviewService)
        {
            _productService = productService;
            _reviewService = reviewService;
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

            var reviewSummary = await _reviewService.GetReviewsSummaryByProductAsync(id);
            ViewBag.ReviewSummary = reviewSummary;

            return View(product);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitReview(HuitShopDB.Models.DTOs.Review.SubmitReviewRequest request)
        {
            // For now, using a hardcoded user ID 1 (until Auth logic is fully integrated)
            int userId = 1; 

            if (ModelState.IsValid)
            {
                bool success = await _reviewService.SubmitReviewAsync(userId, request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Đánh giá của bạn đã được gửi và đang chờ duyệt.";
                }
            }
            return RedirectToAction("Detail", new { id = request.ProductId });
        }
    }
}

