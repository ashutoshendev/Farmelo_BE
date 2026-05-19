using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Queries.Business;
using Farmelo.Business.Queries.Dashboard;
using Farmelo.Shared.CommonHelper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Authorize(Roles = AppConstants.Roles.Admin + "," + AppConstants.Roles.Owner)]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController : ApiBaseController<AdminDashboardController>
{
    public AdminDashboardController(ILogger<AdminDashboardController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet("summary")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
        => Ok(await Mediator.Send(new GetAdminDashboardSummaryQuery(), ct));

    [HttpGet("business-summary")]
    public async Task<IActionResult> GetBusinessSummary(CancellationToken ct)
        => Ok(await Mediator.Send(new GetBusinessDashboardQuery(), ct));
}
