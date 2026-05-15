using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Commands.Owners;
using Farmelo.Business.Queries.Owners;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Owners;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Authorize(Roles = AppConstants.Roles.Admin + "," + AppConstants.Roles.Owner)]
[Route("api/admin/owners")]
public sealed class AdminOwnersController : ApiBaseController<AdminOwnersController>
{
    public AdminOwnersController(ILogger<AdminOwnersController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetOwners(CancellationToken ct)
        => Ok(await Mediator.Send(new GetOwnersQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> CreateOwner([FromBody] OwnerCreateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<OwnerDto>();
        }

        var result = await Mediator.Send(new CreateOwnerCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{ownerId:int}")]
    public async Task<IActionResult> UpdateOwner(
        int ownerId,
        [FromBody] OwnerUpdateRequestDto? request,
        CancellationToken ct)
    {
        if (ownerId <= 0)
        {
            return InvalidRouteIdResult<OwnerDto>(nameof(ownerId));
        }

        if (request == null)
        {
            return MissingBodyResult<OwnerDto>();
        }

        var result = await Mediator.Send(new UpdateOwnerCommand(ownerId, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{ownerId:int}")]
    public async Task<IActionResult> DeactivateOwner(int ownerId, CancellationToken ct)
    {
        if (ownerId <= 0)
        {
            return InvalidRouteIdResult<OwnerDto>(nameof(ownerId));
        }

        var result = await Mediator.Send(new DeactivateOwnerCommand(ownerId), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
