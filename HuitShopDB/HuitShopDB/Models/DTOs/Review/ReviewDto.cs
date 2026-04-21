using System;
using System.Collections.Generic;

namespace HuitShopDB.Models.DTOs.Review
{
    public class ReviewDto
    {
        public ReviewDto()
        {
            ImageUrls = new List<string>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatarUrl { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? VariantId { get; set; }
        public string VariantName { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }
        public int HelpfulCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> ImageUrls { get; set; }
        public ReviewResponseDto AdminResponse { get; set; }
    }

    public class ProductReviewSummaryDto
    {
        public ProductReviewSummaryDto()
        {
            RatingDistribution = new Dictionary<int, int>();
            LatestReviews = new List<ReviewDto>();
        }

        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; }
        public List<ReviewDto> LatestReviews { get; set; }
    }

    public class SubmitReviewRequest
    {
        public SubmitReviewRequest()
        {
            ImageUrls = new List<string>();
        }

        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<string> ImageUrls { get; set; }
    }

    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public int AdminId { get; set; }
        public string AdminName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddReviewResponseRequest
    {
        public int ReviewId { get; set; }
        public string Content { get; set; }
    }

    public class ReviewApprovalRequest
    {
        public int ReviewId { get; set; }
        public bool IsApproved { get; set; }
        public string RejectionReason { get; set; }
    }

    public class ReviewAnalyticsDto
    {
        public ReviewAnalyticsDto()
        {
            RatingDistribution = new Dictionary<int, int>();
            Trends = new List<ReviewTrendDto>();
        }

        public int TotalReviews { get; set; }
        public int PendingReviews { get; set; }
        public int ApprovedReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; }
        public List<ReviewTrendDto> Trends { get; set; }
        public int MostReviewedProductId { get; set; }
        public string MostReviewedProductName { get; set; }
    }

    public class ReviewTrendDto
    {
        public DateTime Date { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }
}
