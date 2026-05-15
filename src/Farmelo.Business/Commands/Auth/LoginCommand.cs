using Farmelo.Shared.DTO.Auth;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Commands.Auth;

public sealed record LoginCommand(LoginRequestDto Request)
    : IRequest<ServiceOperationResult<LoginResponseDto>>;
