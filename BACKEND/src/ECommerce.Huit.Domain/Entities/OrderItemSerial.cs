using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class OrderItemSerial : BaseEntity
    {
        public OrderItemSerial()
        {
            SerialNumber = string.Empty;
        }

        public int OrderItemId { get; set; }
        public string SerialNumber { get; set; }

        // Navigation properties
        public virtual OrderItem OrderItem { get; set; }
    }
}
