using Dapper;
using Farmelo.Business.Queries.Audit;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Audit;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Audit;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, ServiceOperationResult<AuditLogPageDto>>,
      IRequestHandler<GetAuditActorsQuery, ServiceOperationResult<IReadOnlyList<AuditActorDto>>>
{
    private readonly IDapperExecutor _dapper;
    private readonly ICurrentUser _currentUser;

    public GetAuditLogsQueryHandler(IDapperExecutor dapper, ICurrentUser currentUser)
    {
        _dapper = dapper;
        _currentUser = currentUser;
    }

    public async Task<ServiceOperationResult<AuditLogPageDto>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Filter.Page);
        var pageSize = Math.Clamp(request.Filter.PageSize, 5, 100);
        var skip = (page - 1) * pageSize;
        var searchTerm = string.IsNullOrWhiteSpace(request.Filter.Search)
            ? null
            : $"%{request.Filter.Search.Trim()}%";
        var module = string.IsNullOrWhiteSpace(request.Filter.Module) ? null : request.Filter.Module.Trim();
        var actionType = string.IsNullOrWhiteSpace(request.Filter.ActionType) ? null : request.Filter.ActionType.Trim();
        var from = request.Filter.From.HasValue
            ? IndianDateTimeHelper.ToUtcStartOfIndianDay(request.Filter.From.Value)
            : (DateTime?)null;
        var to = request.Filter.To.HasValue
            ? IndianDateTimeHelper.ToUtcEndOfIndianDay(request.Filter.To.Value)
            : (DateTime?)null;
        var scopedActorId = IsOwner() ? _currentUser.UserId : request.Filter.ActorId;

        var result = await _dapper.QueryMultipleAsync(
            """
            SELECT Id,
                   UserId AS ActorId,
                   UserFullName AS ActorName,
                   UserRole AS ActorRole,
                   EventType AS ActionType,
                   Module,
                   TargetId,
                   ISNULL(TargetLabel, '') AS TargetLabel,
                   ISNULL(OldValue, '{}') AS OldValue,
                   ISNULL(NewValue, '{}') AS NewValue,
                   IpAddress,
                   OccurredOn AS Timestamp
            FROM AuditLogs
            WHERE EventType <> 'ApiRequest'
              AND EventType <> 'ModuleView'
              AND (@ActorId IS NULL OR UserId = @ActorId)
              AND (@Module IS NULL OR Module = @Module)
              AND (@ActionType IS NULL OR EventType = @ActionType)
              AND (@From IS NULL OR OccurredOn >= @From)
              AND (@To IS NULL OR OccurredOn <= @To)
              AND (
                    @SearchTerm IS NULL
                    OR UserFullName LIKE @SearchTerm
                    OR ISNULL(TargetLabel, '') LIKE @SearchTerm
                    OR Module LIKE @SearchTerm
                    OR EventType LIKE @SearchTerm
                  )
            ORDER BY OccurredOn DESC, Id DESC
            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(1)
            FROM AuditLogs
            WHERE EventType <> 'ApiRequest'
              AND EventType <> 'ModuleView'
              AND (@ActorId IS NULL OR UserId = @ActorId)
              AND (@Module IS NULL OR Module = @Module)
              AND (@ActionType IS NULL OR EventType = @ActionType)
              AND (@From IS NULL OR OccurredOn >= @From)
              AND (@To IS NULL OR OccurredOn <= @To)
              AND (
                    @SearchTerm IS NULL
                    OR UserFullName LIKE @SearchTerm
                    OR ISNULL(TargetLabel, '') LIKE @SearchTerm
                    OR Module LIKE @SearchTerm
                    OR EventType LIKE @SearchTerm
                  );
            """,
            new
            {
                ActorId = scopedActorId,
                Module = module,
                ActionType = actionType,
                From = from,
                To = to,
                SearchTerm = searchTerm,
                Skip = skip,
                PageSize = pageSize
            },
            async reader =>
            {
                var items = (await reader.ReadAsync<AuditLogDto>()).AsList();
                var totalCount = await reader.ReadFirstAsync<int>();
                return new AuditLogPageDto
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(result);
    }

    public async Task<ServiceOperationResult<IReadOnlyList<AuditActorDto>>> Handle(
        GetAuditActorsQuery request,
        CancellationToken cancellationToken)
    {
        var scopedActorId = IsOwner() ? _currentUser.UserId : (int?)null;
        var actors = await _dapper.QueryAsync<AuditActorDto>(
            """
            SELECT DISTINCT
                   UserId AS ActorId,
                   UserFullName AS ActorName,
                   UserRole AS ActorRole
            FROM AuditLogs
            WHERE UserId IS NOT NULL
              AND EventType <> 'ApiRequest'
              AND EventType <> 'ModuleView'
              AND (@ActorId IS NULL OR UserId = @ActorId)
            ORDER BY UserFullName
            """,
            new { ActorId = scopedActorId },
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(actors);
    }

    private bool IsOwner()
        => string.Equals(_currentUser.UserRole, AppConstants.Roles.Owner, StringComparison.OrdinalIgnoreCase);
}
