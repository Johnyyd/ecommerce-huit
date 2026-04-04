using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Review()
        {
            Content = string.Empty;
            IsVerifiedPurchase = false;
            IsApproved = false;
        }

        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Rating { get; set; } // 1-5
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public bool IsApproved { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductVariant Variant { get; set; }
    }
}
