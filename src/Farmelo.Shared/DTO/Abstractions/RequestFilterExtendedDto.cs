namespace Farmelo.Shared.DTO.Abstractions;

public abstract class RequestFilterExtendedDto : RequestFilterDto
{
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
