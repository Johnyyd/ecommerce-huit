using System;
using System.Collections.Generic;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class Warehouse : BaseEntity
    {
        public Warehouse()
        {
            Code = string.Empty;
            Name = string.Empty;
            Type = WarehouseType.PHYSICAL;
            IsActive = true;
            Inventories = new List<Inventory>();
            Serials = new List<ProductSerial>();
            StockMovements = new List<StockMovement>();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public WarehouseType Type { get; set; }
        public string Phone { get; set; }
        public string Manager { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<ProductSerial> Serials { get; set; }
        public virtual ICollection<StockMovement> StockMovements { get; set; }
    }
}
