using ECommerce.Huit.Application.DTOs.Order;
using FluentValidation;

namespace ECommerce.Huit.Application.Validators.Order;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.ShippingAddressJson)
            .NotEmpty().WithMessage("Địa chỉ giao hàng là bắt buộc")
            .Must(BeValidJson).WithMessage("Địa chỉ giao hàng phải là JSON hợp lệ");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Phương thức thanh toán là bắt buộc")
            .Must(p => new[] { "CASH", "MOMO", "VNPAY", "BANKING", "COD" }.Contains(p))
            .WithMessage("Phương thức thanh toán không hợp lệ");
    }

    private bool BeValidJson(string json)
    {
        try
        {
            _ = System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
