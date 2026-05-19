using Farmelo.Business.Queries.Health;
using Farmelo.Shared.DTO.Health;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Health;

public sealed class GetHealthStatusQueryHandler
    : IRequestHandler<GetHealthStatusQuery, ServiceOperationResult<HealthStatusDto>>
{
    public Task<ServiceOperationResult<HealthStatusDto>> Handle(
        GetHealthStatusQuery request,
        CancellationToken cancellationToken)
    {
        var payload = new HealthStatusDto(
            "Healthy",
            "Farmelo.API",
            DateTimeOffset.UtcNow);

        return Task.FromResult(ServiceOperationResult.CreateWithSuccess(payload));
    }
}
