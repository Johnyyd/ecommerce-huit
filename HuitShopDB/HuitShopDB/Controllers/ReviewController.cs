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

        // GET: /Review/
        public async Task<ActionResult> Index(bool? approved, int? minRating)
        {
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
            if (string.IsNullOrEmpty(request.Content))
            {
                TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống.";
                return RedirectToAction("Index");
            }

            // Hardcoded admin ID 1 for now (as per ProductController pattern)
            int adminId = 1;

            bool success = await _reviewService.AddReviewResponseAsync(request.ReviewId, request, adminId);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã gửi phản hồi thành công.";
            }

            return RedirectToAction("Index");
        }
    }
}
