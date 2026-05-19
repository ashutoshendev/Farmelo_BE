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
[Route("api/inventory")]
public sealed class InventoryController : ApiBaseController<InventoryController>
{
    public InventoryController(ILogger<InventoryController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
        => Ok(await Mediator.Send(new GetInventorySummaryQuery(), ct));

    [HttpGet("raw-stock")]
    public async Task<IActionResult> GetRawStock(CancellationToken ct)
        => Ok(await Mediator.Send(new GetRawStockEntriesQuery(), ct));

    [HttpPost("raw-stock")]
    public async Task<IActionResult> AddRawStock([FromBody] RawStockEntryCreateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<RawStockEntryDto>();
        }

        var result = await Mediator.Send(new AddRawStockCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("box-production")]
    public async Task<IActionResult> CreateBoxProduction([FromBody] BoxProductionCreateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<BoxStockSummaryDto>();
        }

        var result = await Mediator.Send(new CreateBoxProductionCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
