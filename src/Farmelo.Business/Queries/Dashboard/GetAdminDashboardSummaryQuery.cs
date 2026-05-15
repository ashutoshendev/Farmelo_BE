using Farmelo.Shared.DTO.Dashboard;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Queries.Dashboard;

public sealed record GetAdminDashboardSummaryQuery
    : IRequest<ServiceOperationResult<AdminDashboardSummaryDto>>;
