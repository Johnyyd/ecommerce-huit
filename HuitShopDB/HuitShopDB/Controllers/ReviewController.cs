using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using HuitShopDB.Services.Interfaces;
using HuitShopDB.Models.DTOs.Review;

namespace HuitShopDB.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController()
        {
            _reviewService = new Services.ReviewService();
        }

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private bool IsAdminOrStaff()
        {
            string role = Session["UserRole"] as string;
            return role == "ADMIN" || role == "STAFF";
        }

        // GET: /Review/
        public async Task<ActionResult> Index(bool? approved, int? minRating)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Title = "Quản lý đánh giá";
            ViewBag.ApprovedFilter = approved;
            ViewBag.MinRating = minRating;

            var reviews = await _reviewService.GetAllReviewsAsync(approved, minRating);
            return View(reviews);
        }

        // POST: /Review/Approve/5
        [HttpPost]
        public async Task<ActionResult> Approve(int id)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            bool success = await _reviewService.ApproveReviewAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã duyệt đánh giá thành công.";
            }
            return RedirectToAction("Index");
        }

        // POST: /Review/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            bool success = await _reviewService.DeleteReviewAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã xóa đánh giá.";
            }
            return RedirectToAction("Index");
        }

        // POST: /Review/Reply
        [HttpPost]
        public async Task<ActionResult> Reply(AddReviewResponseRequest request)
        {
            if (!IsAdminOrStaff())
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị này.";
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(request.Content))
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
                return RedirectToAction("Index");
            }

            int adminId = Session["UserId"] != null ? (int)Session["UserId"] : 1;

            bool success = await _reviewService.AddReviewResponseAsync(request.ReviewId, request, adminId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã gửi phản hồi thành công.";
            }

            return RedirectToAction("Index");
        }
    }
}
