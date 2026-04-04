using System;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Payment()
        {
            PaymentGateway = string.Empty;
            Fee = 0;
            Status = PaymentStatus.PENDING;
        }

        public int OrderId { get; set; }
        public string PaymentGateway { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public string WebhookData { get; set; } // JSON raw response

        // Navigation properties
        public virtual Order Order { get; set; }
    }
}
