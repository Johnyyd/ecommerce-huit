using System;
using System.Collections.Generic;

namespace ECommerce.Huit.Domain.Entities
{
    public class Permission : BaseEntity
    {
        public Permission()
        {
            Code = string.Empty;
            Name = string.Empty;
            Module = string.Empty;
            RolePermissions = new List<RolePermission>();
        }

        public string Code { get; set; } // e.g., 'products.read'
        public string Name { get; set; }
        public string Module { get; set; } // 'PRODUCT', 'ORDER', ...

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
