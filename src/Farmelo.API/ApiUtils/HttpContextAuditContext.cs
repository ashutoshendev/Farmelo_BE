using Farmelo.Data.Write.Abstractions;

namespace Farmelo.API.ApiUtils;

public sealed class HttpContextAuditContext : ICurrentUser, IAuditContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAuditContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
        => _httpContextAccessor.HttpContext == null
            ? 0
            : ApiHelper.GetUserIdFromClaims(_httpContextAccessor.HttpContext);

    public string UserName
        => _httpContextAccessor.HttpContext == null
            ? string.Empty
            : ApiHelper.GetUserNameFromClaims(_httpContextAccessor.HttpContext);

    public string UserRole
        => _httpContextAccessor.HttpContext == null
            ? string.Empty
            : ApiHelper.GetUserRoleFromClaims(_httpContextAccessor.HttpContext);

    public string? Endpoint
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            var (controller, action) = ApiHelper.GetApiEndPointInformation(_httpContextAccessor.HttpContext);
            return string.IsNullOrWhiteSpace(controller) ? null : $"{controller}/{action}";
        }
    }

    public string? IpAddress
        => _httpContextAccessor.HttpContext == null
            ? null
            : ApiHelper.GetIpAddress(_httpContextAccessor.HttpContext);
}
