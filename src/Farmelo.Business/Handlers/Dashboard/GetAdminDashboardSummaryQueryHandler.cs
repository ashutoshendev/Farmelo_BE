using Farmelo.Business.Queries.Dashboard;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Shared.DTO.Dashboard;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Dashboard;

public sealed class GetAdminDashboardSummaryQueryHandler
    : IRequestHandler<GetAdminDashboardSummaryQuery, ServiceOperationResult<AdminDashboardSummaryDto>>
{
    private readonly IDapperExecutor _dapper;

    public GetAdminDashboardSummaryQueryHandler(IDapperExecutor dapper)
    {
        _dapper = dapper;
    }

    public async Task<ServiceOperationResult<AdminDashboardSummaryDto>> Handle(
        GetAdminDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await _dapper.QuerySingleAsync<AdminDashboardSummaryDto>(
            """
            SELECT
                (SELECT COUNT(1) FROM UserAccounts WHERE Role = 'Owner') AS TotalOwners,
                (SELECT COUNT(1) FROM UserAccounts WHERE Role = 'Owner' AND IsActive = 1) AS ActiveOwners,
                (SELECT COUNT(1) FROM Products WHERE IsActive = 1) AS ActiveProducts,
                (
                    SELECT COUNT(1)
                    FROM AuditLogs
                    WHERE OccurredOn >= DATEADD(day, -7, SYSUTCDATETIME())
                      AND EventType <> 'ApiRequest'
                      AND EventType <> 'ModuleView'
                ) AS RecentActions
            """,
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(summary ?? new AdminDashboardSummaryDto());
    }
}
