using Farmelo.Business.Queries.Owners;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Owners;

public sealed class GetOwnersQueryHandler
    : IRequestHandler<GetOwnersQuery, ServiceOperationResult<IReadOnlyList<OwnerDto>>>
{
    private readonly IDapperExecutor _dapper;

    public GetOwnersQueryHandler(IDapperExecutor dapper)
    {
        _dapper = dapper;
    }

    public async Task<ServiceOperationResult<IReadOnlyList<OwnerDto>>> Handle(
        GetOwnersQuery request,
        CancellationToken cancellationToken)
    {
        var owners = await _dapper.QueryAsync<OwnerDto>(
            """
            SELECT Id, FullName, Email, IsActive, CreatedOn, ModifiedOn
            FROM UserAccounts
            WHERE Role = @Role
            ORDER BY CreatedOn DESC, Id DESC
            """,
            new { Role = AppConstants.Roles.Owner },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(owners);
    }
}
