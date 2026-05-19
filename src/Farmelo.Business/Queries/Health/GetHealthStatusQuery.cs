using Farmelo.Shared.DTO.Health;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Queries.Health;

public sealed record GetHealthStatusQuery : IRequest<ServiceOperationResult<HealthStatusDto>>;
