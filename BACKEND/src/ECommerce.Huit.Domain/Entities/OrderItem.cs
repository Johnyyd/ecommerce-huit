using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public OrderItem()
        {
            ProductName = string.Empty;
            Sku = string.Empty;
            DiscountAmount = 0;
            OrderItemSerials = new List<OrderItemSerial>();
        }

        public int OrderId { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; } // snapshot
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual ProductVariant Variant { get; set; }
        public virtual ICollection<OrderItemSerial> OrderItemSerials { get; set; }
    }
}
