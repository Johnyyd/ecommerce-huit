using System.Collections.Generic;
using System.Threading.Tasks;
using HuitShopDB.Models.DTOs.Review;

namespace HuitShopDB.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ProductReviewSummaryDto> GetReviewsSummaryByProductAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync(bool? isApproved, int? minRating);
        Task<bool> SubmitReviewAsync(int userId, SubmitReviewRequest request);
        Task<bool> ApproveReviewAsync(int reviewId);
        Task<bool> DeleteReviewAsync(int reviewId);
        
        // New methods for enhanced features
        Task<ReviewDto> GetReviewByIdAsync(int reviewId);
        Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(int userId);
        Task<bool> UpdateReviewAsync(int reviewId, SubmitReviewRequest request);
        Task<bool> AddReviewResponseAsync(int reviewId, AddReviewResponseRequest request, int adminId);
        Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync();
        Task<bool> MarkReviewAsHelpfulAsync(int reviewId);
    }
}
