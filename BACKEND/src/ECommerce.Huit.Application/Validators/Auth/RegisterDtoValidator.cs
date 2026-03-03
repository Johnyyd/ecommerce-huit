using ECommerce.Huit.Application.DTOs.Auth;
using FluentValidation;

namespace ECommerce.Huit.Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên là bắt buộc")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email là bắt buộc")
            .EmailAddress().WithMessage("Địa chỉ email không hợp lệ")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Số điện thoại tối đa 20 ký tự")
            .Matches(@"^[0-9]*$").WithMessage("Số điện thoại chỉ chứa số")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu là bắt buộc")
            .MinimumLength(6).WithMessage("Mật khẩu tối thiểu 6 ký tự")
            .MaximumLength(100);
    }
}
