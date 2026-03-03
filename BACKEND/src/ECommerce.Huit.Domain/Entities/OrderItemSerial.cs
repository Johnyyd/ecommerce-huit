namespace ECommerce.Huit.Domain.Entities;

public class OrderItemSerial : BaseEntity
{
    public int OrderItemId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;

    // Navigation properties
    public virtual OrderItem OrderItem { get; set; } = null!;
}
