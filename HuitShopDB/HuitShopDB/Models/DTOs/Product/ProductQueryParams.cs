using System;
using System.ComponentModel.DataAnnotations;

namespace HuitShopDB.Models.DTOs.Product
{
    public class ProductQueryParams
    {
        public ProductQueryParams()
        {
            Page = 1;
            PageSize = 20;
            SortBy = "newest";
            InStockOnly = false;
        }

        [Range(1, int.MaxValue, ErrorMessage = "Page phải lớn hơn 0")]
        public int Page { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PageSize phải lớn hơn 0")]
        public int PageSize { get; set; }

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Search { get; set; }
        public string SortBy { get; set; } // "price_asc", "price_desc", "newest", "name"
        public bool InStockOnly { get; set; }
    }
}

