namespace ECommerce.Huit.Domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty; // e.g., 'products.read'
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty; // 'PRODUCT', 'ORDER', ...

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
