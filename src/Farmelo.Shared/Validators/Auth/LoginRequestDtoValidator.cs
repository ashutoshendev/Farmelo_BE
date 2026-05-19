using Farmelo.Shared.DTO.Auth;
using FluentValidation;

namespace Farmelo.Shared.Validators.Auth;

public sealed class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(200);
    }
}
