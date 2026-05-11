using Farmelo.API.ApiUtils;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.Config;

namespace Farmelo.API.Middleware;

public sealed class RequestLoggingMiddleware
{
    private const string InfoMessage = "API call {State} in Farmelo.API.Controllers.{ControllerName}-{MethodName} | CorrelationId: {CorrelationId} | UserEmail:{Email}";
    private const string ErrorMessage = "Error occurred in Farmelo.RequestLogging.Middleware.{Exception} | UserEmail:{Email}";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly bool _logEnabled;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        ConfigurationOptions configurationOptions)
    {
        _next = next;
        _logger = logger;
        _logEnabled = configurationOptions.ToggleSettings.RequestLogEnabled;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var userEmail = string.Empty;
        var controllerName = string.Empty;
        var actionName = string.Empty;
        var correlationId = string.Empty;

        try
        {
            if (_logEnabled)
            {
                correlationId = Guid.NewGuid().ToString();
                (controllerName, actionName) = ApiHelper.GetApiEndPointInformation(httpContext);
                userEmail = ApiHelper.GetUserContext(httpContext).Email;

                _logger.LogInformation(InfoMessage, "start", controllerName, actionName, correlationId, userEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessage, MethodHelper.FormatException(ex), userEmail);
        }

        await _next(httpContext);

        if (_logEnabled)
        {
            _logger.LogInformation(InfoMessage, "end", controllerName, actionName, correlationId, userEmail);
        }
    }
}
