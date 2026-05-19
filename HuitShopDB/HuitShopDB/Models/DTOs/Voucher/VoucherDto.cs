using System;
using System.ComponentModel.DataAnnotations;

namespace HuitShopDB.Models.DTOs.Voucher
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

    public class VoucherCreateDto
    {
        public VoucherCreateDto()
        {
            Code = string.Empty;
            Name = string.Empty;
            DiscountType = "PERCENT";
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(30);
            UsagePerUser = 1;
            MinOrderValue = 0;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [StringLength(50, ErrorMessage = "Mã tối đa 50 ký tự")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Tên voucher không được để trống")]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Loại giảm giá không được để trống")]
        public string DiscountType { get; set; }

        [Required]
        [Range(0.01, 100000000, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        public decimal DiscountValue { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        [Range(0, 100000000, ErrorMessage = "Đơn hàng tối thiểu không hợp lệ")]
        public decimal MinOrderValue { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue)]
        public int UsageLimit { get; set; }

        [Range(1, 100)]
        public int UsagePerUser { get; set; }
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
