namespace Farmelo.Shared.DTO.Abstractions;

public abstract class RequestFilterDto
{
    public int PageNo { get; set; }
    public int PageSize { get; set; }
    public string? SearchQuery { get; set; }
}
