namespace Farmelo.Shared.DTO.Owners;

public sealed class OwnerUpdateRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public bool IsActive { get; set; } = true;
}
