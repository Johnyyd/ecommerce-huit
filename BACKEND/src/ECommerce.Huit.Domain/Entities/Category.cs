using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Domain.Entities
{
    public class Category : BaseEntity
    {
        public Category()
        {
            Name = string.Empty;
            Slug = string.Empty;
            IsActive = true;
            SortOrder = 0;
            Children = new List<Category>();
            Products = new List<Product>();
        }

        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }

        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
