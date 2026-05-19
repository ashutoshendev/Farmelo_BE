using Farmelo.Shared.DTO.Auth;

namespace Farmelo.API.Services.Auth;

public interface IJwtTokenService
{
    JwtLoginToken CreateToken(AuthUserDto user, bool rememberMe);
}

public sealed record JwtLoginToken(string AccessToken, DateTimeOffset ExpiresAtUtc);
