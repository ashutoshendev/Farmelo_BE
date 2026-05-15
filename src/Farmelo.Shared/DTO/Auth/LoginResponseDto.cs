namespace Farmelo.Shared.DTO.Auth;

public sealed class LoginResponseDto
{
    public AuthUserDto User { get; set; } = new();
}
