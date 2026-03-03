using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities;

public class SupportTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.MEDIUM;
    public TicketStatus Status { get; set; } = TicketStatus.OPEN;
    public int? AssignedTo { get; set; }
    public int? OrderId { get; set; }
    public int? ProductId { get; set; }
    public DateTime? LastMessageAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
}
