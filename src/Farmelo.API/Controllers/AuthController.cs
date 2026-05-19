using Farmelo.API.Auditing;
using Farmelo.API.Controllers.Abstractions;
using Farmelo.API.Services.Auth;
using Farmelo.Business.Commands.Auth;
using Farmelo.Business.Queries.Auth;
using Farmelo.Shared.DTO.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Farmelo.API.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiBaseController<AuthController>
{
    private readonly IAuditLogger _auditLogger;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        ILogger<AuthController> logger,
        IMediator mediator,
        IAuditLogger auditLogger,
        IJwtTokenService jwtTokenService)
        : base(logger, mediator)
    {
        _auditLogger = auditLogger;
        _jwtTokenService = jwtTokenService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<LoginResponseDto>();
        }

        var result = await Mediator.Send(new LoginCommand(request), ct);
        if (!result.Success || result.Payload == null)
        {
            return Unauthorized(result);
        }

        var user = result.Payload.User;
        var token = _jwtTokenService.CreateToken(user, request.RememberMe);
        result.Payload.AccessToken = token.AccessToken;
        result.Payload.ExpiresAtUtc = token.ExpiresAtUtc;

        _auditLogger.LogAuthenticationEvent("LOGIN", user.Id, user.FullName);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
            ? parsedUserId
            : 0;
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        _auditLogger.LogAuthenticationEvent("LOGOUT", userId, userName);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(), ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
