using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class OrderStatusHistory : BaseEntity
    {
        public OrderStatusHistory()
        {
            Status = string.Empty;
        }

        public int OrderId { get; set; }
        public string Status { get; set; }
        public int? ChangedBy { get; set; }
        public string Note { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual User ChangedByUser { get; set; }
    }
}
