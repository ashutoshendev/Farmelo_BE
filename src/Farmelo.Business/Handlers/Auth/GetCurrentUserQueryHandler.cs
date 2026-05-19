using Farmelo.Business.Queries.Auth;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Shared.DTO.Auth;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Auth;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, ServiceOperationResult<AuthUserDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IDapperExecutor _dapper;

    public GetCurrentUserQueryHandler(ICurrentUser currentUser, IDapperExecutor dapper)
    {
        _currentUser = currentUser;
        _dapper = dapper;
    }

    public async Task<ServiceOperationResult<AuthUserDto>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId <= 0)
        {
            return ServiceOperationResult.CreateWithFailure<AuthUserDto>("User is not authenticated.");
        }

        var user = await _dapper.QuerySingleAsync<AuthUserDto>(
            """
            SELECT Id, FullName, Email, Role
            FROM UserAccounts
            WHERE Id = @UserId AND IsActive = 1
            """,
            new { _currentUser.UserId },
            ct: cancellationToken);

        return user == null
            ? ServiceOperationResult.CreateWithFailure<AuthUserDto>("User is inactive or does not exist.")
            : ServiceOperationResult.CreateWithSuccess(user);
    }
}
