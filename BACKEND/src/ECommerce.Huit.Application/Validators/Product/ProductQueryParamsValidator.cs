using ECommerce.Huit.Application.DTOs.Product;
using FluentValidation;

namespace ECommerce.Huit.Application.Validators.Product;

public class ProductQueryParamsValidator : AbstractValidator<ProductQueryParams>
{
    public ProductQueryParamsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page phải lớn hơn 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize phải lớn hơn 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize tối đa 100");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("MinPrice phải >= 0")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0).WithMessage("MaxPrice phải > 0")
            .Must((model, maxPrice) => !maxPrice.HasValue || maxPrice >= model.MinPrice.GetValueOrDefault())
            .WithMessage("MaxPrice phải lớn hơn hoặc bằng MinPrice")
            .When(x => x.MaxPrice.HasValue);
    }
}
