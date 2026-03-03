namespace ECommerce.Huit.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Origin { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
