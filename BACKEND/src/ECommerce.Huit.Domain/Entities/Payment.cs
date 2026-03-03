using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities;

public class Payment : BaseEntity
{
    public int OrderId { get; set; }
    public string PaymentGateway { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; } = 0;
    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
    public DateTime? PaidAt { get; set; }
    public string? WebhookData { get; set; } // JSON raw response

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}
