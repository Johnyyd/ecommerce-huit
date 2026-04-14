using System;
using System.Collections.Generic;

namespace HuitShopDB.Models.DTOs.Review
{
    public class ReviewDto
    {
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
        public DateTime CreatedAt { get; set; }
    }

    public class SubmitReviewRequest
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class ProductReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewDto> LatestReviews { get; set; }
    }
}
