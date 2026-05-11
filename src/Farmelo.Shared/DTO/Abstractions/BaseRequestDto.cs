using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Farmelo.Shared.DTO.Abstractions;

public abstract class BaseRequestDto
{
    protected BaseRequestDto()
    {
    }

    protected BaseRequestDto(IHttpContextAccessor httpContextAccessor)
    {
        CreatedBy = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ModifiedBy = CreatedBy;
    }

    [JsonIgnore]
    public int CompanyId { get; set; }

    [JsonIgnore]
    public int ChannelId { get; set; }

    [JsonIgnore]
    public string? CreatedBy { get; set; }

    [JsonIgnore]
    public string? ModifiedBy { get; set; }
}
