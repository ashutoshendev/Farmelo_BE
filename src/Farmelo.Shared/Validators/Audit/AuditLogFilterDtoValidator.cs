using Farmelo.Shared.DTO.Audit;
using FluentValidation;

namespace Farmelo.Shared.Validators.Audit;

public sealed class AuditLogFilterDtoValidator : AbstractValidator<AuditLogFilterDto>
{
    public AuditLogFilterDtoValidator()
    {
        RuleFor(x => x.Search).MaximumLength(120);
        RuleFor(x => x.Module).MaximumLength(120);
        RuleFor(x => x.ActionType).MaximumLength(40);
        RuleFor(x => x.ActorId).GreaterThan(0).When(x => x.ActorId.HasValue);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(5, 100);
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue);
    }
}
