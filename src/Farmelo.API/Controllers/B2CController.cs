using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Commands.Business;
using Farmelo.Business.Queries.Business;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Business;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Authorize(Roles = AppConstants.Roles.Admin + "," + AppConstants.Roles.Owner)]
[Route("api/b2c")]
public sealed class B2CController : ApiBaseController<B2CController>
{
    public B2CController(ILogger<B2CController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments([FromQuery] int? partyId, CancellationToken ct)
        => Ok(await Mediator.Send(new GetB2CAssignmentsQuery(partyId), ct));

    [HttpPost("assignments")]
    public async Task<IActionResult> CreateAssignment([FromBody] B2CAssignmentCreateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<B2CAssignmentDto>();
        }

        var result = await Mediator.Send(new CreateB2CAssignmentCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("returns")]
    public async Task<IActionResult> ReturnStock([FromBody] B2CReturnRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<B2CAssignmentDto>();
        }

        var result = await Mediator.Send(new ReturnB2CStockCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
