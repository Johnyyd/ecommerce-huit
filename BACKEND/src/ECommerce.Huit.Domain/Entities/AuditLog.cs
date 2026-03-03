namespace ECommerce.Huit.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public int? ChangedBy { get; set; }
    public string? IpAddress { get; set; }

    // Navigation properties
    public virtual User? ChangedByUser { get; set; }
}
