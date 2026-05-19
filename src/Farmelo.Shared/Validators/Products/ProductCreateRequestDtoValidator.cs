using Farmelo.Shared.DTO.Products;
using Farmelo.Shared.Validators;
using FluentValidation;

namespace Farmelo.Shared.Validators.Products;

public sealed class ProductCreateRequestDtoValidator : AbstractValidator<ProductCreateRequestDto>
{
    public ProductCreateRequestDtoValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120).Matches(ValidationPatterns.Slug);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(600);
        RuleFor(x => x.Weight).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WeightGrams).GreaterThan(0).LessThanOrEqualTo(100_000);
        RuleFor(x => x.CurrentPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(99_999_999.99m);
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches(ValidationPatterns.CurrencyCode);
        RuleFor(x => x.Ingredients).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Nutrition).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
        RuleFor(x => x.AccentColor)
            .MaximumLength(20)
            .Matches(ValidationPatterns.HexColor)
            .When(x => !string.IsNullOrWhiteSpace(x.AccentColor));
        RuleFor(x => x.BackgroundColor)
            .MaximumLength(20)
            .Matches(ValidationPatterns.HexColor)
            .When(x => !string.IsNullOrWhiteSpace(x.BackgroundColor));
        RuleFor(x => x.PriceChangeReason).MaximumLength(300);
    }
}
