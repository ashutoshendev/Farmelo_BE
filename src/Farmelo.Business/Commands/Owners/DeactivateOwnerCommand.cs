using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Commands.Owners;

public sealed record DeactivateOwnerCommand(int OwnerId)
    : IRequest<ServiceOperationResult<OwnerDto>>;
