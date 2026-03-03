namespace ECommerce.Huit.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxCode { get; set; }
    public string? BankAccount { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
