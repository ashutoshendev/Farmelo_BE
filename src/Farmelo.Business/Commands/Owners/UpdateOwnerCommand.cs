using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Commands.Owners;

public sealed record UpdateOwnerCommand(int OwnerId, OwnerUpdateRequestDto Request)
    : IRequest<ServiceOperationResult<OwnerDto>>;
