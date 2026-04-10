using System;
using System.Collections.Generic;

namespace HuitShopDB.Models.DTOs.Product
{
    public class ProductDetailDto
    {
        public ProductDetailDto()
        {
            Name = string.Empty;
            Slug = string.Empty;
            Variants = new List<ProductVariantDto>();
            Images = new List<ProductImageDto>();
            Reviews = new List<ReviewDto>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Specifications { get; set; } // JSON
        public BrandDto Brand { get; set; }
        public CategoryDto Category { get; set; }
        public List<ProductVariantDto> Variants { get; set; }
        public List<ProductImageDto> Images { get; set; }
        public List<ReviewDto> Reviews { get; set; }
        public decimal RatingAverage { get; set; }
        public int ReviewCount { get; set; }
    }

    public class ProductVariantDto
    {
        public ProductVariantDto()
        {
            Sku = string.Empty;
        }

        public int Id { get; set; }
        public string Sku { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string ThumbnailUrl { get; set; }
        public int QuantityAvailable { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductImageDto
    {
        public ProductImageDto()
        {
            ImageUrl = string.Empty;
        }

        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public int SortOrder { get; set; }
    }

    public class ReviewDto
    {
        public ReviewDto()
        {
            Content = string.Empty;
        }

        public int Id { get; set; }
        public UserSimpleDto User { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserSimpleDto
    {
        public UserSimpleDto()
        {
            FullName = string.Empty;
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
    }
}

