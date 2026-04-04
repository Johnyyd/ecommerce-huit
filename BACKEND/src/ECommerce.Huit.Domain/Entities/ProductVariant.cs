using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public ProductVariant()
        {
            Sku = string.Empty;
            DisplayOrder = 0;
            IsActive = true;
            Images = new List<ProductImage>();
            Inventories = new List<Inventory>();
            Serials = new List<ProductSerial>();
            CartItems = new List<CartItem>();
            OrderItems = new List<OrderItem>();
        }

        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public string ThumbnailUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public int? WeightGrams { get; set; }
        public string Dimensions { get; set; } // JSON: {"length":...,"width":...,"height":...}

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<ProductSerial> Serials { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
