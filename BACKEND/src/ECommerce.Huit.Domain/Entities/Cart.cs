namespace ECommerce.Huit.Domain.Entities;

public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public string? VoucherCode { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
