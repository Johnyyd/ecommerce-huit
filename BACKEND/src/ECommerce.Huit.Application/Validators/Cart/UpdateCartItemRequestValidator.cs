using ECommerce.Huit.Application.DTOs.Cart;
using FluentValidation;

namespace ECommerce.Huit.Application.Validators.Cart;

public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0")
            .LessThanOrEqualTo(100).WithMessage("Số lượng tối đa 100");
    }
}