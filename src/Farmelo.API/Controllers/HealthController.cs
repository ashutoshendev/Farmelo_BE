using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Queries.Health;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Route("api/health")]
public sealed class HealthController : ApiBaseController<HealthController>
{
    public HealthController(ILogger<HealthController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetHealthStatusQuery(), ct));
}
