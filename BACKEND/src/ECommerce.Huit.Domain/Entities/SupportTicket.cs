using System;
using ECommerce.Huit.Domain.Enums;

namespace ECommerce.Huit.Domain.Entities
{
    public class SupportTicket : BaseEntity
    {
        public SupportTicket()
        {
            TicketNumber = string.Empty;
            Subject = string.Empty;
            Priority = TicketPriority.MEDIUM;
            Status = TicketStatus.OPEN;
        }

        public string TicketNumber { get; set; }
        public int UserId { get; set; }
        public string Subject { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketStatus Status { get; set; }
        public int? AssignedTo { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public DateTime? LastMessageAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual User AssignedToUser { get; set; }
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
