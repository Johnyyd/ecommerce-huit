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
    }
}
