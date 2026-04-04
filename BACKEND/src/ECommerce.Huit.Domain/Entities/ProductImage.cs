using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public ProductImage()
        {
            ImageUrl = string.Empty;
            SortOrder = 0;
        }

        public int VariantId { get; set; }
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public int SortOrder { get; set; }

        public virtual ProductVariant Variant { get; set; }
    }
}
