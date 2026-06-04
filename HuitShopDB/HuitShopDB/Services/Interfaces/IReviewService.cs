using System.Collections.Generic;
using HuitShopDB.Models.DTOs.Review;

namespace HuitShopDB.Services.Interfaces
{
    public interface IReviewService
    {
        ProductReviewSummaryDto GetReviewsSummaryByProduct(int productId);
        IEnumerable<ReviewDto> GetAllReviews(bool? isApproved, int? minRating);
        bool SubmitReview(int userId, SubmitReviewRequest request);
        bool ApproveReview(int reviewId);
        bool DeleteReview(int reviewId);
        
        // New methods for enhanced features
        ReviewDto GetReviewById(int reviewId);
        IEnumerable<ReviewDto> GetUserReviews(int userId);
        bool UpdateReview(int reviewId, SubmitReviewRequest request);
        bool AddReviewResponse(int reviewId, AddReviewResponseRequest request, int adminId);
        ReviewAnalyticsDto GetReviewAnalytics();
        bool MarkReviewAsHelpful(int reviewId);
    }
}

