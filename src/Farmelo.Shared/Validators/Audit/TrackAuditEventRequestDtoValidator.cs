using Farmelo.Shared.DTO.Audit;
using FluentValidation;

namespace Farmelo.Shared.Validators.Audit;

public sealed class TrackAuditEventRequestDtoValidator : AbstractValidator<TrackAuditEventRequestDto>
{
    public TrackAuditEventRequestDtoValidator()
    {
        RuleFor(x => x.Module).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Action).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Details).MaximumLength(1000);
    }
}
