using System;

namespace ECommerce.Huit.Application.DTOs.Voucher
{
    public class VoucherDto
    {
        public VoucherDto()
        {
            Code = string.Empty;
            Name = string.Empty;
            DiscountType = string.Empty;
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsagePerUser { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class ValidateVoucherRequest
    {
        public ValidateVoucherRequest()
        {
            Code = string.Empty;
        }

        public string Code { get; set; }
    }

    public class ValidateVoucherResponse
    {
        public ValidateVoucherResponse()
        {
        }

        public bool Valid { get; set; }
        public string Reason { get; set; }
        public VoucherDto Voucher { get; set; }
    }
}
