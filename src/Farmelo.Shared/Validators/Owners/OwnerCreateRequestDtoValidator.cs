using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.Validators;
using FluentValidation;

namespace Farmelo.Shared.Validators.Owners;

public sealed class OwnerCreateRequestDtoValidator : AbstractValidator<OwnerCreateRequestDto>
{
    public OwnerCreateRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(200)
            .Matches(ValidationPatterns.StrongPassword)
            .WithMessage("Password must contain uppercase, lowercase, number and special character.");
    }
}
