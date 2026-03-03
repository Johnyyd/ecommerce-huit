namespace ECommerce.Huit.Domain.Entities;

public class VoucherUsage : BaseEntity
{
    public int VoucherId { get; set; }
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public decimal DiscountAmount { get; set; }

    // Navigation properties
    public virtual Voucher Voucher { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
}
