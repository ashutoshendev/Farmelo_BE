using Farmelo.Shared.Config;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Farmelo.API.Services.Auth;

public static class JwtTokenOptions
{
    public static SymmetricSecurityKey CreateSigningKey(AuthOptions authOptions)
    {
        if (string.IsNullOrWhiteSpace(authOptions.JwtSigningKey))
        {
            throw new InvalidOperationException("AuthOptions:JwtSigningKey is required for JWT authentication.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(authOptions.JwtSigningKey);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("AuthOptions:JwtSigningKey must be at least 32 bytes.");
        }

        return new SymmetricSecurityKey(keyBytes);
    }

    public static string GetIssuer(AuthOptions authOptions)
        => string.IsNullOrWhiteSpace(authOptions.JwtIssuer)
            ? "Farmelo"
            : authOptions.JwtIssuer;

    public static string GetAudience(AuthOptions authOptions)
        => string.IsNullOrWhiteSpace(authOptions.JwtAudience)
            ? "Farmelo.Client"
            : authOptions.JwtAudience;
}
