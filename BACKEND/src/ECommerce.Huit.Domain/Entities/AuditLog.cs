using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public AuditLog()
        {
            TableName = string.Empty;
            Operation = string.Empty;
        }

        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Operation { get; set; } // INSERT, UPDATE, DELETE
        public string OldValues { get; set; } // JSON
        public string NewValues { get; set; } // JSON
        public int? ChangedBy { get; set; }
        public string IpAddress { get; set; }

        // Navigation properties
        public virtual User ChangedByUser { get; set; }
    }
}
