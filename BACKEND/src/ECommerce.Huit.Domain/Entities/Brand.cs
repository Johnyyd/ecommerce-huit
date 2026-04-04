using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Domain.Entities
{
    public class Brand : BaseEntity
    {
        public Brand()
        {
            Name = string.Empty;
            Products = new List<Product>();
        }

        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string Origin { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
