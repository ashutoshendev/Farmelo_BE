using Farmelo.Shared.DTO.Audit;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Queries.Audit;

public sealed record GetAuditLogsQuery(AuditLogFilterDto Filter)
    : IRequest<ServiceOperationResult<AuditLogPageDto>>;

public sealed record GetAuditActorsQuery
    : IRequest<ServiceOperationResult<IReadOnlyList<AuditActorDto>>>;
