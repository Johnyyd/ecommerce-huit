using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities;

public class Order : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public int UserId { get; set; }
    public OrderType OrderType { get; set; } = OrderType.ONLINE;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; } = 0;
    public decimal ShippingFee { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.PENDING;
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public string ShippingAddress { get; set; } = string.Empty; // JSON
    public string? Note { get; set; }
    public string? StaffNote { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
    public virtual Payment? Payment { get; set; }
}
