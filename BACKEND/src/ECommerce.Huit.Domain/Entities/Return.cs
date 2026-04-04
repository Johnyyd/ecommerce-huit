using System;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class Return : BaseEntity
    {
        public Return()
        {
            ReturnNumber = string.Empty;
            Reason = string.Empty;
            Status = ReturnStatus.REQUESTED;
        }

        public string ReturnNumber { get; set; }
        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; }
        public ReturnStatus Status { get; set; }
        public decimal? RefundAmount { get; set; }
        public string RefundMethod { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual OrderItem OrderItem { get; set; }
        public virtual User User { get; set; }
    }
}
