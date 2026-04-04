using System;
using System.Collections.Generic;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class Voucher : BaseEntity
    {
        public Voucher()
        {
            Code = string.Empty;
            Name = string.Empty;
            MinOrderValue = 0;
            UsagePerUser = 1;
            UsageCount = 0;
            IsActive = true;
            Usages = new List<VoucherUsage>();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderValue { get; set; }
        public string ApplicableProductIds { get; set; } // JSON array
        public string ApplicableCategoryIds { get; set; } // JSON array
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsagePerUser { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<VoucherUsage> Usages { get; set; }
    }
}
