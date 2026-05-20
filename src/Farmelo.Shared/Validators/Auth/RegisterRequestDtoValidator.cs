using Farmelo.Shared.DTO.Auth;
using FluentValidation;

namespace Farmelo.Shared.Validators.Auth;

public sealed class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches(ValidationPatterns.StrongPassword)
            .WithMessage("Password must contain uppercase, lowercase, number and special character.");
    }
}
