using Farmelo.API.Controllers.Abstractions;
using Farmelo.API.Services.Notifications;
using Farmelo.Shared.DTO.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[AllowAnonymous]
[Route("api/notifications")]
public sealed class NotificationsController : ApiBaseController<NotificationsController>
{
    private readonly IInquiryNotificationService _notificationService;

    public NotificationsController(
        ILogger<NotificationsController> logger,
        IMediator mediator,
        IInquiryNotificationService notificationService)
        : base(logger, mediator)
    {
        _notificationService = notificationService;
    }

    [HttpPost("inquiries")]
    public async Task<IActionResult> SendInquiry([FromBody] InquiryNotificationRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        var result = await _notificationService.SendAsync(request, ct);
        if (result.EmailSent || result.WhatsAppSent)
        {
            return Ok(result);
        }

        var message = result.EmailError ?? result.WhatsAppError ?? "Notification could not be sent.";
        return BadRequest(new
        {
            message,
            result.EmailSent,
            result.WhatsAppSent,
            result.EmailError,
            result.WhatsAppError
        });
    }
}
