using System;

namespace ECommerce.Huit.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        public RolePermission()
        {
            Role = string.Empty;
        }

        public string Role { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public virtual Permission Permission { get; set; }
    }
}
