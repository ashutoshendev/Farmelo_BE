namespace Farmelo.Shared.DTO.Owners;

public sealed class OwnerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
