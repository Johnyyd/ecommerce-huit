using System;
using System.Collections.Generic;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class ProductSerial : BaseEntity
    {
        public ProductSerial()
        {
            SerialNumber = string.Empty;
            Status = SerialStatus.AVAILABLE;
            InboundDate = DateTime.UtcNow;
            OrderItemSerials = new List<OrderItemSerial>();
        }

        public int VariantId { get; set; }
        public string SerialNumber { get; set; }
        public int WarehouseId { get; set; }
        public SerialStatus Status { get; set; }
        public DateTime InboundDate { get; set; }
        public DateTime? OutboundDate { get; set; }
        public DateTime? WarrantyExpireDate { get; set; } // DateOnly changed to DateTime? for .NET 4.5.1
        public string Notes { get; set; }

        // Navigation properties
        public virtual ProductVariant Variant { get; set; }
        public virtual Warehouse Warehouse { get; set; }
        public virtual ICollection<OrderItemSerial> OrderItemSerials { get; set; }
    }
}
