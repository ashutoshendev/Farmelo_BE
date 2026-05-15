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
[Route("api/payments")]
public sealed class PaymentsController : ApiBaseController<PaymentsController>
{
    public PaymentsController(ILogger<PaymentsController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] int? partyId, CancellationToken ct)
        => Ok(await Mediator.Send(new GetPaymentsQuery(partyId), ct));

    [HttpGet("pending-summary")]
    public async Task<IActionResult> GetPendingSummary(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPaymentSummaryQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> RecordPayment([FromBody] PaymentCreateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<PaymentDto>();
        }

        var result = await Mediator.Send(new CreatePaymentCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
