using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Queries.Audit;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Audit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Authorize(Roles = AppConstants.Roles.Admin + "," + AppConstants.Roles.Owner)]
[Route("api/admin/audit")]
public sealed class AdminAuditController : ApiBaseController<AdminAuditController>
{
    public AdminAuditController(ILogger<AdminAuditController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterDto filter, CancellationToken ct)
    {
        filter ??= new AuditLogFilterDto();

        var result = await Mediator.Send(new GetAuditLogsQuery(filter), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("actors")]
    public async Task<IActionResult> GetAuditActors(CancellationToken ct)
        => Ok(await Mediator.Send(new GetAuditActorsQuery(), ct));
}
