using Farmelo.Shared.DTO.Auth;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Queries.Auth;

public sealed record GetCurrentUserQuery : IRequest<ServiceOperationResult<AuthUserDto>>;
