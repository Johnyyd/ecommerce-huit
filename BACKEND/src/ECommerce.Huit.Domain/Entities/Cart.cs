using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public Cart()
        {
            Items = new List<CartItem>();
        }

        public int UserId { get; set; }
        public string VoucherCode { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<CartItem> Items { get; set; }
    }
}
