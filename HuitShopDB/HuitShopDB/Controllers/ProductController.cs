using System;
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

        public ActionResult Index(int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, string search, string sortBy, bool inStockOnly = false, int page = 1)
        {
            var query = new ProductQueryParams();
            query.CategoryId = categoryId;
            query.BrandId = brandId;
            query.MinPrice = minPrice;
            query.MaxPrice = maxPrice;
            query.Search = search;
            query.SortBy = sortBy ?? "newest";
            query.InStockOnly = inStockOnly;
            query.Page = page;
            query.PageSize = 12;

            var products = _productService.GetProducts(query);
            var totalCount = _productService.GetProductsCount(query);
            var categories = _productService.GetCategories();
            var brands = _productService.GetBrands();

            ViewBag.Categories = categories;
            ViewBag.Brands = brands;
            ViewBag.QueryParams = query;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

            // Maintain backward compatibility for standard parameters
            ViewBag.CurrentCategory = categoryId;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = query.SortBy;

            return View(products);
        }

        [HttpGet]
        public JsonResult InstantSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new { success = false, data = new object[0] }, JsonRequestBehavior.AllowGet);
            }

            var query = new ProductQueryParams
            {
                Search = term,
                Page = 1,
                PageSize = 5
            };

            var products = _productService.GetProducts(query);
            
            var results = new System.Collections.Generic.List<object>();
            foreach (var p in products)
            {
                results.Add(new {
                    id = p.Id,
                    name = p.Name,
                    price = p.PriceFrom,
                    img = p.ThumbnailUrl
                });
            }

            return Json(new { success = true, data = results }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail(int id)
        {
            var product = _productService.GetProductDetail(id);
            if (product == null) return HttpNotFound();

            var reviewSummary = _reviewService.GetReviewsSummaryByProduct(id);
            ViewBag.ReviewSummary = reviewSummary;

            return View(product);
        }

        [HttpPost]
        public ActionResult SubmitReview(HuitShopDB.Models.DTOs.Review.SubmitReviewRequest request)
        {
            int userId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để gửi đánh giá.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                bool success = _reviewService.SubmitReview(userId, request);
                if (success)
                {
                    TempData["SuccessMessage"] = "Đánh giá của bạn đã được gửi và đang chờ duyệt.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể gửi đánh giá. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi hệ thống khi lưu đánh giá. Vui lòng thử lại sau.";
                // Log exception if needed: System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            
            return RedirectToAction("Detail", new { id = request.ProductId });
        }

        // ==========================================
        // ADMIN PRODUCT MANAGEMENT MODULE (PHASE 4)
        // ==========================================

        private bool IsAdminOrStaff()
        {
            string role = Session["UserRole"] as string;
            return role == "ADMIN" || role == "STAFF";
        }

        private string UploadImageFile(System.Web.HttpPostedFileBase uploadFile)
        {
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                try
                {
                    string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(uploadFile.FileName);
                    string uploadDir = Server.MapPath("~/Uploads/");
                    if (!System.IO.Directory.Exists(uploadDir))
                    {
                        System.IO.Directory.CreateDirectory(uploadDir);
                    }
                    string path = System.IO.Path.Combine(uploadDir, fileName);
                    uploadFile.SaveAs(path);
                    return "/Uploads/" + fileName;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        [HttpGet]
        public ActionResult AdminIndex(string search, int? categoryId, string status, int page = 1)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            int pageSize = 10;
            var products = _productService.GetAdminProducts(search, categoryId, status, page, pageSize);
            int totalProducts = _productService.GetAdminProductsCount(search, categoryId, status);
            var categories = _productService.GetCategories();

            ViewBag.Categories = categories;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Status = status ?? "ALL";
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);
            ViewBag.TotalProducts = totalProducts;

            return View(products);
        }

        [HttpGet]
        public ActionResult Create()
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Categories = _productService.GetCategories();
            ViewBag.Brands = _productService.GetBrands();

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(ProductCreateDto model, System.Web.HttpPostedFileBase uploadImage)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Tên sản phẩm không được để trống.");
            }

            if (ModelState.IsValid)
            {
                string imageUrl = UploadImageFile(uploadImage);
                model.DefaultThumbnailUrl = imageUrl ?? "/Content/images/placeholder.png";

                int productId = _productService.CreateProduct(model);
                if (productId > 0)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                    return RedirectToAction("Edit", new { id = productId });
                }
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu sản phẩm.";
            }

            ViewBag.Categories = _productService.GetCategories();
            ViewBag.Brands = _productService.GetBrands();
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            var product = _productService.GetAdminProductDetail(id);
            if (product == null) return HttpNotFound();

            ViewBag.Categories = _productService.GetCategories();
            ViewBag.Brands = _productService.GetBrands();

            return View(product);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int id, ProductEditDto model)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                bool success = _productService.UpdateProduct(id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin sản phẩm thành công!";
                    return RedirectToAction("Edit", new { id = id });
                }
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu thông tin sản phẩm.";
            }

            var product = _productService.GetAdminProductDetail(id);
            ViewBag.Categories = _productService.GetCategories();
            ViewBag.Brands = _productService.GetBrands();
            return View(product);
        }

        [HttpPost]
        public JsonResult AddVariant(int productId, string variantName, string sku, decimal price, decimal originalPrice, System.Web.HttpPostedFileBase variantImage)
        {
            if (!IsAdminOrStaff())
            {
                return Json(new { success = false, message = "Bạn không có quyền truy cập." });
            }

            string imageUrl = UploadImageFile(variantImage) ?? "/Content/images/placeholder.png";

            var dto = new VariantCreateDto
            {
                VariantName = variantName,
                Sku = sku,
                Price = price,
                OriginalPrice = originalPrice,
                ThumbnailUrl = imageUrl,
                IsActive = true,
                DisplayOrder = 1
            };

            bool success = _productService.CreateVariant(productId, dto);
            if (success)
            {
                return Json(new { success = true, message = "Thêm biến thể mới thành công!" });
            }
            return Json(new { success = false, message = "Không thể lưu biến thể." });
        }

        [HttpPost]
        public JsonResult UpdateVariant(int variantId, string variantName, string sku, decimal price, decimal originalPrice, bool isActive, int displayOrder, System.Web.HttpPostedFileBase variantImage)
        {
            if (!IsAdminOrStaff())
            {
                return Json(new { success = false, message = "Bạn không có quyền truy cập." });
            }

            string imageUrl = UploadImageFile(variantImage);

            var dto = new VariantEditDto
            {
                VariantName = variantName,
                Sku = sku,
                Price = price,
                OriginalPrice = originalPrice,
                ThumbnailUrl = imageUrl,
                IsActive = isActive,
                DisplayOrder = displayOrder
            };

            bool success = _productService.UpdateVariant(variantId, dto);
            if (success)
            {
                return Json(new { success = true, message = "Cập nhật biến thể thành công!" });
            }
            return Json(new { success = false, message = "Không thể lưu biến thể." });
        }

        [HttpPost]
        public JsonResult ToggleProductStatus(int productId, string status)
        {
            if (!IsAdminOrStaff())
            {
                return Json(new { success = false, message = "Bạn không có quyền truy cập." });
            }

            bool success = _productService.ToggleProductStatus(productId, status);
            if (success)
            {
                return Json(new { success = true, message = "Cập nhật trạng thái sản phẩm thành công!" });
            }
            return Json(new { success = false, message = "Không thể cập nhật trạng thái." });
        }

        [HttpPost]
        public JsonResult AddProductImage(int variantId, string altText, int sortOrder, System.Web.HttpPostedFileBase galleryImage)
        {
            if (!IsAdminOrStaff())
            {
                return Json(new { success = false, message = "Bạn không có quyền truy cập." });
            }

            string imageUrl = UploadImageFile(galleryImage);
            if (string.IsNullOrEmpty(imageUrl))
            {
                return Json(new { success = false, message = "Vui lòng chọn hình ảnh hợp lệ để tải lên." });
            }

            bool success = _productService.AddProductImage(variantId, imageUrl, altText, sortOrder);
            if (success)
            {
                return Json(new { success = true, message = "Tải lên ảnh bộ sưu tập thành công!" });
            }
            return Json(new { success = false, message = "Không thể lưu ảnh bộ sưu tập." });
        }

        [HttpPost]
        public JsonResult DeleteProductImage(int imageId)
        {
            if (!IsAdminOrStaff())
            {
                return Json(new { success = false, message = "Bạn không có quyền truy cập." });
            }

            bool success = _productService.DeleteProductImage(imageId);
            if (success)
            {
                return Json(new { success = true, message = "Xóa ảnh thành công!" });
            }
            return Json(new { success = false, message = "Không thể xóa ảnh." });
        }
    }
}


