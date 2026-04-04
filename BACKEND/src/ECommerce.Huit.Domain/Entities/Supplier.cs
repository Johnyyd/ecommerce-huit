using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public Supplier()
        {
            Code = string.Empty;
            Name = string.Empty;
            IsActive = true;
            StockMovements = new List<StockMovement>();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }
        public string BankAccount { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<StockMovement> StockMovements { get; set; }
    }
}
