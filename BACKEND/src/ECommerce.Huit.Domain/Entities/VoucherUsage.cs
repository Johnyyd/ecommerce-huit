using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class VoucherUsage : BaseEntity
    {
        public VoucherUsage()
        {
        }

        public int VoucherId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public decimal DiscountAmount { get; set; }

        // Navigation properties
        public virtual Voucher Voucher { get; set; }
        public virtual User User { get; set; }
        public virtual Order Order { get; set; }
    }
}
