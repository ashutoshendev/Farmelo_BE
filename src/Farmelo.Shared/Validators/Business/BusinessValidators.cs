using Farmelo.Shared.DTO.Business;
using FluentValidation;

namespace Farmelo.Shared.Validators.Business;

public sealed class PartyWriteRequestDtoValidator : AbstractValidator<PartyWriteRequestDto>
{
    public PartyWriteRequestDtoValidator()
    {
        RuleFor(x => x.PartyType).NotEmpty().MaximumLength(30).Must(BeKnownPartyType)
            .WithMessage("Party type must be B2B, Shop, Distributor or Seller.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(180);
        RuleFor(x => x.ContactName).MaximumLength(150);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Location).MaximumLength(250);
    }

    private static bool BeKnownPartyType(string value)
        => BusinessValidationValues.PartyTypes.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}

public sealed class RawStockEntryCreateRequestDtoValidator : AbstractValidator<RawStockEntryCreateRequestDto>
{
    public RawStockEntryCreateRequestDtoValidator()
    {
        RuleFor(x => x.SellerPartyId).GreaterThan(0).When(x => x.SellerPartyId.HasValue);
        RuleFor(x => x.SuttaGrade).NotEmpty().MaximumLength(40).Must(BusinessValidationValues.BeKnownSuttaGrade)
            .WithMessage("Sutta grade is invalid.");
        RuleFor(x => x.QuantityKg).GreaterThan(0).LessThanOrEqualTo(999_999);
        RuleFor(x => x.CostPerKg).GreaterThanOrEqualTo(0).LessThanOrEqualTo(9_999_999);
        RuleFor(x => x.SupplierName).MaximumLength(180);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class BoxProductionCreateRequestDtoValidator : AbstractValidator<BoxProductionCreateRequestDto>
{
    public BoxProductionCreateRequestDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.SuttaGrade).NotEmpty().MaximumLength(40).Must(BusinessValidationValues.BeKnownSuttaGrade)
            .WithMessage("Sutta grade is invalid.");
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(10_000_000);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class B2BOrderCreateRequestDtoValidator : AbstractValidator<B2BOrderCreateRequestDto>
{
    public B2BOrderCreateRequestDtoValidator()
    {
        RuleFor(x => x.PartyId).GreaterThan(0);
        RuleFor(x => x.SuttaGrade).NotEmpty().MaximumLength(40).Must(BusinessValidationValues.BeKnownSuttaGrade)
            .WithMessage("Sutta grade is invalid.");
        RuleFor(x => x.QuantityKg).GreaterThan(0).LessThanOrEqualTo(999_999);
        RuleFor(x => x.PricePerKg).GreaterThanOrEqualTo(0).LessThanOrEqualTo(9_999_999);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.OrderDate)
            .When(x => x.OrderDate.HasValue && x.DueDate.HasValue);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class B2CAssignmentCreateRequestDtoValidator : AbstractValidator<B2CAssignmentCreateRequestDto>
{
    public B2CAssignmentCreateRequestDtoValidator()
    {
        RuleFor(x => x.PartyId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(10_000_000);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(9_999_999).When(x => x.UnitPrice.HasValue);
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.AssignmentDate)
            .When(x => x.AssignmentDate.HasValue && x.DueDate.HasValue);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class B2CReturnRequestDtoValidator : AbstractValidator<B2CReturnRequestDto>
{
    public B2CReturnRequestDtoValidator()
    {
        RuleFor(x => x.AssignmentId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(10_000_000);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public sealed class PaymentCreateRequestDtoValidator : AbstractValidator<PaymentCreateRequestDto>
{
    public PaymentCreateRequestDtoValidator()
    {
        RuleFor(x => x.PaymentType).NotEmpty().MaximumLength(20).Must(BeKnownPaymentType)
            .WithMessage("Payment type must be B2B or B2C.");
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(999_999_999);
        RuleFor(x => x.PaymentMode).MaximumLength(80);
        RuleFor(x => x.ReferenceNumber).MaximumLength(120);
        RuleFor(x => x.Notes).MaximumLength(500);
        RuleFor(x => x.B2BOrderId).GreaterThan(0).When(x => string.Equals(x.PaymentType, "B2B", StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.B2CAssignmentId).GreaterThan(0).When(x => string.Equals(x.PaymentType, "B2C", StringComparison.OrdinalIgnoreCase));
    }

    private static bool BeKnownPaymentType(string value)
        => BusinessValidationValues.PaymentTypes.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}

public sealed class ReportDateRangeRequestDtoValidator : AbstractValidator<ReportDateRangeRequestDto>
{
    public ReportDateRangeRequestDtoValidator()
    {
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("To date must be greater than or equal to from date.");
    }
}

internal static class BusinessValidationValues
{
    public static readonly string[] PartyTypes = ["B2B", "Shop", "Distributor", "Seller"];
    public static readonly string[] PaymentTypes = ["B2B", "B2C"];
    public static readonly string[] SuttaGrades =
    [
        "Makhana 4 Sutta",
        "Makhana 4 Plus Sutta",
        "Makhana 5 Sutta",
        "Makhana 5 Plus Sutta",
        "Makhana 6 Sutta",
        "Makhana 6 Plus Sutta"
    ];

    public static bool BeKnownSuttaGrade(string value)
        => SuttaGrades.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}
