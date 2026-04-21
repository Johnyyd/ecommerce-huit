using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuitShopDB.Models;
using HuitShopDB.Models.DTOs.Review;
using HuitShopDB.Services.Interfaces;

namespace HuitShopDB.Services
{
    public class ReviewService : IReviewService
    {
        private readonly HuitShopDBDataContext _context;

        public ReviewService()
        {
            _context = new HuitShopDBDataContext();
        }

        public async Task<ProductReviewSummaryDto> GetReviewsSummaryByProductAsync(int productId)
        {
            var approvedReviews = _context.reviews
                .Where(r => r.product_id == productId && r.is_approved == true)
                .OrderByDescending(r => r.created_at)
                .ToList();

            var summary = new ProductReviewSummaryDto
            {
                TotalReviews = approvedReviews.Count,
                AverageRating = approvedReviews.Any() ? approvedReviews.Average(r => r.rating) : 0,
                LatestReviews = approvedReviews.Select(r => MapToDto(r)).ToList()
            };

            // Add rating distribution
            for (int i = 1; i <= 5; i++)
            {
                summary.RatingDistribution[i] = approvedReviews.Count(r => r.rating == i);
            }

            return await Task.FromResult(summary);
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync(bool? isApproved, int? minRating)
        {
            var query = _context.reviews.AsQueryable();

            if (isApproved.HasValue)
            {
                query = query.Where(r => r.is_approved == isApproved.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(r => r.rating >= minRating.Value);
            }

            var reviews = query.OrderByDescending(r => r.created_at).ToList();
            var result = reviews.Select(r => MapToDto(r)).ToList();

            return await Task.FromResult(result);
        }

        public async Task<bool> SubmitReviewAsync(int userId, SubmitReviewRequest request)
        {
            var r = new review
            {
                user_id = userId,
                product_id = request.ProductId,
                variant_id = request.VariantId,
                rating = request.Rating,
                title = request.Title,
                content = request.Content,
                is_approved = false, // Moderation required
                is_verified_purchase = false, // Could check orders table here
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            _context.reviews.InsertOnSubmit(r);
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> ApproveReviewAsync(int reviewId)
        {
            var r = _context.reviews.FirstOrDefault(x => x.id == reviewId);
            if (r == null) return false;

            r.is_approved = true;
            r.updated_at = DateTime.Now;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var r = _context.reviews.FirstOrDefault(x => x.id == reviewId);
            if (r == null) return false;

            _context.reviews.DeleteOnSubmit(r);
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<ReviewDto> GetReviewByIdAsync(int reviewId)
        {
            var r = _context.reviews.FirstOrDefault(x => x.id == reviewId);
            if (r == null) return null;

            return await Task.FromResult(MapToDto(r));
        }

        public async Task<IEnumerable<ReviewDto>> GetUserReviewsAsync(int userId)
        {
            var reviews = _context.reviews
                .Where(r => r.user_id == userId)
                .OrderByDescending(r => r.created_at)
                .ToList();

            var result = reviews.Select(r => MapToDto(r)).ToList();
            return await Task.FromResult(result);
        }

        public async Task<bool> UpdateReviewAsync(int reviewId, SubmitReviewRequest request)
        {
            var r = _context.reviews.FirstOrDefault(x => x.id == reviewId);
            if (r == null) return false;

            r.rating = request.Rating;
            r.title = request.Title;
            r.content = request.Content;
            r.updated_at = DateTime.Now;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        public async Task<bool> AddReviewResponseAsync(int reviewId, AddReviewResponseRequest request, int adminId)
        {
            // This assumes there's a review_response table
            // Implementation depends on your database schema
            return await Task.FromResult(true);
        }

        public async Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync()
        {
            var allReviews = _context.reviews.ToList();
            var analytics = new ReviewAnalyticsDto
            {
                TotalReviews = allReviews.Count,
                ApprovedReviews = allReviews.Count(r => r.is_approved),
                PendingReviews = allReviews.Count(r => !r.is_approved),
                AverageRating = allReviews.Any() ? allReviews.Average(r => r.rating) : 0,
                RatingDistribution = new Dictionary<int, int>()
            };

            // Rating distribution
            for (int i = 1; i <= 5; i++)
            {
                analytics.RatingDistribution[i] = allReviews.Count(r => r.rating == i);
            }

            // Most reviewed product
            var mostReviewed = allReviews.GroupBy(r => r.product_id)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            
            if (mostReviewed != null)
            {
                var product = _context.products.FirstOrDefault(p => p.id == mostReviewed.Key);
                if (product != null)
                {
                    analytics.MostReviewedProductId = product.id;
                    analytics.MostReviewedProductName = product.name;
                }
            }

            return await Task.FromResult(analytics);
        }

        public async Task<bool> MarkReviewAsHelpfulAsync(int reviewId)
        {
            var r = _context.reviews.FirstOrDefault(x => x.id == reviewId);
            if (r == null) return false;

            // Assuming there's a helpful_count column
            r.updated_at = DateTime.Now;
            _context.SubmitChanges();

            return await Task.FromResult(true);
        }

        private ReviewDto MapToDto(review r)
        {
            return new ReviewDto
            {
                Id = r.id,
                ProductId = r.product_id,
                ProductName = r.product != null ? r.product.name : "",
                VariantId = r.variant_id,
                VariantName = r.product_variant != null ? r.product_variant.variant_name : null,
                UserId = r.user_id,
                UserName = r.user != null ? r.user.full_name : "",
                UserAvatarUrl = r.user != null ? r.user.avatar_url : null,
                Rating = r.rating,
                Title = r.title,
                Content = r.content,
                IsApproved = r.is_approved,
                IsVerifiedPurchase = r.is_verified_purchase,
                CreatedAt = r.created_at,
                UpdatedAt = r.updated_at
            };
        }
    }
}

