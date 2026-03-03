namespace ECommerce.Huit.Domain.Entities;

public class Address : BaseEntity
{
    public int UserId { get; set; }
    public string Label { get; set; } = string.Empty; // 'Nhà', 'Văn phòng', ...
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;

    public virtual User User { get; set; } = null!;
}
