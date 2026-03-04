using ECommerce.Huit.Application.DTOs.Product;
using ECommerce.Huit.Application.DTOs.Voucher;

namespace ECommerce.Huit.Application.DTOs.Cart;

public class CartDto
{
    public int Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public VoucherDto? AppliedVoucher { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public ProductVariantDto Variant { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class AddCartItemRequest
{
    public int VariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
