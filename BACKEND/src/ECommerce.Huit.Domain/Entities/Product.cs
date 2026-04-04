using System;
using System.Collections.Generic;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Product()
        {
            Name = string.Empty;
            Slug = string.Empty;
            Status = ProductStatus.DRAFT;
            IsFeatured = false;
            Variants = new List<ProductVariant>();
        }

        public string Name { get; set; }
        public string Slug { get; set; }
        public int? BrandId { get; set; }
        public int CategoryId { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; } // HTML
        public string Specifications { get; set; } // JSON string
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public ProductStatus Status { get; set; }
        public bool IsFeatured { get; set; }
        public int CreatedBy { get; set; }

        // Navigation properties
        public virtual Brand Brand { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductVariant> Variants { get; set; }
    }
}
