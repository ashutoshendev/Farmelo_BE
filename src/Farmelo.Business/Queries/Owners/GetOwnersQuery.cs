using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Queries.Owners;

public sealed record GetOwnersQuery : IRequest<ServiceOperationResult<IReadOnlyList<OwnerDto>>>;
