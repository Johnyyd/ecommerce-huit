namespace ECommerce.Huit.Application.DTOs.Product;

public class BrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Origin { get; set; }
    public string? LogoUrl { get; set; }
}
