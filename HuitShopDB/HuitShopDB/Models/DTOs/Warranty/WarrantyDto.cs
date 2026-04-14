using System;

namespace HuitShopDB.Models.DTOs.Warranty
{
    public class WarrantyDto
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string VariantName { get; set; }
        public string CustomerName { get; set; }
        public string OrderCode { get; set; }
        public DateTime? OutboundDate { get; set; } // Purchase Date
        public DateTime? ExpireDate { get; set; }
        public string Status { get; set; } // ACTIVE, EXPIRED, NOT_SOLD
        public int DaysRemaining { get; set; }
        public string Notes { get; set; }
    }
}
