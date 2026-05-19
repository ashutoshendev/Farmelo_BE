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
[Route("api/parties")]
public sealed class PartiesController : ApiBaseController<PartiesController>
{
    public PartiesController(ILogger<PartiesController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetParties([FromQuery] string? partyType, CancellationToken ct)
        => Ok(await Mediator.Send(new GetPartiesQuery(partyType), ct));

    [HttpPost]
    public async Task<IActionResult> CreateParty([FromBody] PartyWriteRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<PartyDto>();
        }

        var result = await Mediator.Send(new CreatePartyCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{partyId:int}")]
    public async Task<IActionResult> UpdateParty(int partyId, [FromBody] PartyWriteRequestDto? request, CancellationToken ct)
    {
        if (partyId <= 0)
        {
            return InvalidRouteIdResult<PartyDto>(nameof(partyId));
        }

        if (request == null)
        {
            return MissingBodyResult<PartyDto>();
        }

        var result = await Mediator.Send(new UpdatePartyCommand(partyId, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
