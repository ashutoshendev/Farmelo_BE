using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Commands.Owners;

public sealed record CreateOwnerCommand(OwnerCreateRequestDto Request)
    : IRequest<ServiceOperationResult<OwnerDto>>;
