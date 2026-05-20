using Farmelo.Shared.DTO.Checkout;
using FluentValidation;

namespace Farmelo.Shared.Validators.Checkout;

public sealed class CheckoutQuoteRequestDtoValidator : AbstractValidator<CheckoutQuoteRequestDto>
{
    public CheckoutQuoteRequestDtoValidator()
    {
        RuleFor(x => x.Items).NotEmpty().Must(x => x.Count <= 50).WithMessage("Cart cannot contain more than 50 items.");
        RuleForEach(x => x.Items).SetValidator(new CheckoutItemRequestDtoValidator());
    }
}

public sealed class CheckoutCreateOrderRequestDtoValidator : AbstractValidator<CheckoutCreateOrderRequestDto>
{
    public CheckoutCreateOrderRequestDtoValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CheckoutItemRequestDtoValidator());
        RuleFor(x => x.ClientTotalAmount).GreaterThan(0).LessThanOrEqualTo(999_999_999);
        RuleFor(x => x.PaymentMethod).NotEmpty().Must(BeKnownPaymentMethod).WithMessage("Payment method must be COD or UPI.");
        RuleFor(x => x.Address.FullName).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Address.Phone).NotEmpty().Matches("^[6-9][0-9]{9}$").WithMessage("Enter a valid 10 digit mobile number.");
        RuleFor(x => x.Address.Email).EmailAddress().MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Address.Email));
        RuleFor(x => x.Address.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Address.City).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Address.Pincode).NotEmpty().Matches("^[0-9]{6}$").WithMessage("Enter a valid 6 digit pincode.");
        RuleFor(x => x.Address.Notes).MaximumLength(500);
    }

    private static bool BeKnownPaymentMethod(string value)
        => value.Equals("COD", StringComparison.OrdinalIgnoreCase)
            || value.Equals("UPI", StringComparison.OrdinalIgnoreCase);
}

public sealed class UpiPaymentUpdateRequestDtoValidator : AbstractValidator<UpiPaymentUpdateRequestDto>
{
    public UpiPaymentUpdateRequestDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty().Must(BeKnownStatus).WithMessage("Status must be Success, Failed, Pending, or Cancelled.");
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(999_999_999);
        RuleFor(x => x.TransactionId).MaximumLength(120);
        RuleFor(x => x.ReferenceId).MaximumLength(120);
        RuleFor(x => x.ProviderResponse).MaximumLength(2000);
    }

    private static bool BeKnownStatus(string value)
        => value.Equals("Success", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Failed", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Pending", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);
}

internal sealed class CheckoutItemRequestDtoValidator : AbstractValidator<CheckoutItemRequestDto>
{
    public CheckoutItemRequestDtoValidator()
    {
        RuleFor(x => x.ProductSlug).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Weight).MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(99);
    }
}
