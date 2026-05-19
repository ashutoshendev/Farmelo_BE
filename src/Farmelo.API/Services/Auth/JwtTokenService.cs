using Farmelo.Shared.Config;
using Farmelo.Shared.DTO.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Farmelo.API.Services.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly ConfigurationOptions _config;

    public JwtTokenService(ConfigurationOptions config)
    {
        _config = config;
    }

    public JwtLoginToken CreateToken(AuthUserDto user, bool rememberMe)
    {
        var authOptions = _config.AuthOptions;
        var expires = rememberMe
            ? DateTimeOffset.UtcNow.AddDays(Math.Max(1, authOptions.RememberMeDays))
            : DateTimeOffset.UtcNow.AddHours(Math.Max(1, authOptions.ExpireHours));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var credentials = new SigningCredentials(
            JwtTokenOptions.CreateSigningKey(authOptions),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtTokenOptions.GetIssuer(authOptions),
            audience: JwtTokenOptions.GetAudience(authOptions),
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return new JwtLoginToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
