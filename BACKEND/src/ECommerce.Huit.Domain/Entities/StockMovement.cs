using System;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class StockMovement : BaseEntity
    {
        public StockMovement()
        {
        }

        public int WarehouseId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; } // positive = in, negative = out
        public MovementType MovementType { get; set; }
        public int? ReferenceId { get; set; } // order_id, purchase_id, etc.
        public string ReferenceType { get; set; } // 'ORDER', 'PURCHASE_ORDER', 'TRANSFER'
        public int? SupplierId { get; set; }
        public string Note { get; set; }
        public int? CreatedBy { get; set; }

        // Navigation properties
        public virtual Warehouse Warehouse { get; set; }
        public virtual ProductVariant Variant { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual User CreatedByUser { get; set; }
    }
}
