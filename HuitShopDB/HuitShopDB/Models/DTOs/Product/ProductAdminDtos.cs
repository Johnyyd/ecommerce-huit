using System;

namespace HuitShopDB.Models.DTOs.Product
{
    public class ProductCreateDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Specifications { get; set; }
        public string Status { get; set; }
        public bool IsFeatured { get; set; }

        // Initial default variant fields
        public string DefaultVariantName { get; set; }
        public string DefaultSku { get; set; }
        public decimal DefaultPrice { get; set; }
        public decimal DefaultOriginalPrice { get; set; }
        public string DefaultThumbnailUrl { get; set; }
    }

    public class ProductEditDto
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Specifications { get; set; }
        public string Status { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class VariantCreateDto
    {
        public string VariantName { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class VariantEditDto
    {
        public string VariantName { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
    }
}
