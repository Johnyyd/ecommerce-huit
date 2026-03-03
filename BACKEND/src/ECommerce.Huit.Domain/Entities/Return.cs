using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities;

public class Return : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ReturnStatus Status { get; set; } = ReturnStatus.REQUESTED;
    public decimal? RefundAmount { get; set; }
    public string? RefundMethod { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual OrderItem OrderItem { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
