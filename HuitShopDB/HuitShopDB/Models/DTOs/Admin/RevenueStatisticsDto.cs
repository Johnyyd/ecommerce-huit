using System;
using System.Collections.Generic;

namespace HuitShopDB.Models.DTOs.Admin
{
    public class RevenueStatisticsDto
    {
        public RevenueStatisticsDto()
        {
            DailyRevenue = new List<DailyRevenueDto>();
            TopSellingProducts = new List<TopProductDto>();
            TopCategories = new List<TopCategoryDto>();
            OrderStatusCounts = new Dictionary<string, int>();
            DatePreset = "THIS_MONTH";
        }

        public decimal TotalRevenue { get; set; } // Completed orders
        public decimal PendingRevenue { get; set; } // Pending + Confirmed + Shipping orders
        public int TotalOrders { get; set; } // Total orders in period
        public int CompletedOrdersCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int CancelledOrdersCount { get; set; }
        public int TotalProductsSold { get; set; } // Sum of quantity in completed orders
        public decimal AverageOrderValue { get; set; } // TotalRevenue / CompletedOrdersCount
        public decimal TotalDiscount { get; set; } // Sum of discount in completed orders
        
        public List<DailyRevenueDto> DailyRevenue { get; set; }
        public List<TopProductDto> TopSellingProducts { get; set; }
        public List<TopCategoryDto> TopCategories { get; set; }
        public Dictionary<string, int> OrderStatusCounts { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DatePreset { get; set; }
    }

    public class DailyRevenueDto
    {
        public string Date { get; set; } // Format: yyyy-MM-dd
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class TopProductDto
    {
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalSales { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class TopCategoryDto
    {
        public string CategoryName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalSales { get; set; }
    }
}
