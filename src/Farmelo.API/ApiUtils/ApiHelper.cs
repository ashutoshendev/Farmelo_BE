using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Security.Claims;
using System.Text.Json;

namespace Farmelo.API.ApiUtils;

public static class ApiHelper
{
    internal static THeaderType? GetHeaderValue<THeaderType>(string headerKey, HttpContext httpContext)
        where THeaderType : class
    {
        if (string.IsNullOrEmpty(httpContext.Request.Headers[headerKey]))
        {
            return default;
        }

        string? headerValue = httpContext.Request.Headers[headerKey];
        return string.IsNullOrEmpty(headerValue) ? default : headerValue.Convert<THeaderType>();
    }

    internal static string GetAuthorizationToken(HttpContext context)
        => context.Request.Headers[AppConstants.ApiHeaders.Authorization].ToString();

    internal static string ConstructHttpResponse(int statusCode, string message, string? additionalInformation = "")
        => JsonSerializer.Serialize(new ErrorDetails
        {
            StatusCode = statusCode,
            Message = message,
            ApiException = additionalInformation
        });

    internal static string GetIpAddress(HttpContext httpContext)
        => httpContext.Request.Headers["X-Forwarded-For"].ToString() ?? string.Empty;

    internal static (string ControllerName, string ActionMethod) GetApiEndPointInformation(HttpContext context)
    {
        var descriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        return descriptor == null
            ? (string.Empty, string.Empty)
            : (descriptor.ControllerName, descriptor.ActionName);
    }

    internal static UserContext GetUserContext(HttpContext httpContext)
    {
        var zippedUserContext = GetHeaderValue<string>(AppConstants.ApiHeaders.UserId, httpContext);

        if (!string.IsNullOrEmpty(zippedUserContext))
        {
            var userContext = MethodHelper.FromJsonStringToObject<UserContext>(MethodHelper.Unzip(zippedUserContext));
            if (userContext != null)
            {
                return userContext;
            }
        }

        return new UserContext();
    }

    internal static int GetUserIdFromClaims(HttpContext httpContext)
    {
        var userIdValue = GetClaimValue(httpContext, ClaimTypes.NameIdentifier, "sub", "userId");
        return int.TryParse(userIdValue, out var userId) ? userId : 0;
    }

    internal static string GetUserNameFromClaims(HttpContext httpContext)
        => GetClaimValue(httpContext, ClaimTypes.Name, "name", "username", "email") ?? string.Empty;

    internal static string GetUserRoleFromClaims(HttpContext httpContext)
        => GetClaimValue(httpContext, ClaimTypes.Role, "role") ?? string.Empty;

    private static string? GetClaimValue(HttpContext httpContext, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = httpContext.User.FindFirstValue(claimType);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}

internal sealed class ErrorDetails
{
    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public string? ApiException { get; set; }
}
