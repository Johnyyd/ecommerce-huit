using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class Inventory : BaseEntity
    {
        public Inventory()
        {
            QuantityOnHand = 0;
            QuantityReserved = 0;
            ReorderPoint = 10;
        }

        public int WarehouseId { get; set; }
        public int VariantId { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int ReorderPoint { get; set; }

        // Navigation properties
        public virtual Warehouse Warehouse { get; set; }
        public virtual ProductVariant Variant { get; set; }
    }
}
