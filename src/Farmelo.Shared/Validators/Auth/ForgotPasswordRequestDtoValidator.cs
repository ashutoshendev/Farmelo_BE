using Farmelo.Shared.DTO.Auth;
using FluentValidation;

namespace Farmelo.Shared.Validators.Auth;

public sealed class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
