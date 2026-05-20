using Farmelo.Shared.DTO.Auth;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Commands.Auth;

public sealed record RegisterCommand(RegisterRequestDto Request)
    : IRequest<ServiceOperationResult<LoginResponseDto>>;
