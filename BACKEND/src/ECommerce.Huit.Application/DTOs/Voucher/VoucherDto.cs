namespace ECommerce.Huit.Application.DTOs.Voucher;

public class VoucherDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
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
    public string Code { get; set; } = string.Empty;
}

public class ValidateVoucherResponse
{
    public bool Valid { get; set; }
    public string? Reason { get; set; }
    public VoucherDto? Voucher { get; set; }
}
