namespace Farmelo.Shared.DTO.Auth;

public sealed class LoginResponseDto
{
    public AuthUserDto User { get; set; } = new();
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
