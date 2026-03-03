namespace ECommerce.Huit.Domain.Entities;

public class RolePermission : BaseEntity
{
    public string Role { get; set; } = string.Empty;
    public int PermissionId { get; set; }

    // Navigation properties
    public virtual Permission Permission { get; set; } = null!;
}
