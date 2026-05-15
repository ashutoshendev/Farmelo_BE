using Farmelo.API.Auditing;
using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Commands.Auth;
using Farmelo.Business.Queries.Auth;
using Farmelo.Shared.Config;
using Farmelo.Shared.DTO.Auth;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Farmelo.API.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiBaseController<AuthController>
{
    private readonly ConfigurationOptions _config;
    private readonly IAuditLogger _auditLogger;

    public AuthController(
        ILogger<AuthController> logger,
        IMediator mediator,
        ConfigurationOptions config,
        IAuditLogger auditLogger)
        : base(logger, mediator)
    {
        _config = config;
        _auditLogger = auditLogger;
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
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var expires = request.RememberMe
            ? DateTimeOffset.UtcNow.AddDays(Math.Max(1, _config.AuthOptions.RememberMeDays))
            : DateTimeOffset.UtcNow.AddHours(Math.Max(1, _config.AuthOptions.ExpireHours));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = expires
        });
        HttpContext.User = principal;

        _auditLogger.LogAuthenticationEvent("LOGIN", user.Id, user.FullName);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
            ? parsedUserId
            : 0;
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        _auditLogger.LogAuthenticationEvent("LOGOUT", userId, userName);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
