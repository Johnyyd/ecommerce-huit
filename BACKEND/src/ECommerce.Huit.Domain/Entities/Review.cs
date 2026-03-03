namespace ECommerce.Huit.Domain.Entities;

public class Review : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; } = false;
    public bool IsApproved { get; set; } = false;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ProductVariant? Variant { get; set; }
}
