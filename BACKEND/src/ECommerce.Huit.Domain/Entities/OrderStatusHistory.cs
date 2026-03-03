namespace ECommerce.Huit.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ChangedBy { get; set; }
    public string? Note { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual User? ChangedByUser { get; set; }
}
