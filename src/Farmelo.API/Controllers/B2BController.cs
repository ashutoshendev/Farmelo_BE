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
[Route("api/b2b")]
public sealed class B2BController : ApiBaseController<B2BController>
{
    public B2BController(ILogger<B2BController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] int? partyId, CancellationToken ct)
        => Ok(await Mediator.Send(new GetB2BOrdersQuery(partyId), ct));

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] B2BOrderCreateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<B2BOrderDto>();
        }

        var result = await Mediator.Send(new CreateB2BOrderCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
