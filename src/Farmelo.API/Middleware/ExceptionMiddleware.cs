using Farmelo.API.ApiUtils;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.Config;
using System.Net;

namespace Farmelo.API.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly bool _stackTraceEnabled;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        ConfigurationOptions configurationOptions)
    {
        _next = next;
        _logger = logger;
        _stackTraceEnabled = configurationOptions.ToggleSettings.DisplayStackTrace;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (AggregateException ex)
        {
            foreach (var innerException in ex.InnerExceptions)
            {
                HandleException(httpContext, innerException);
            }

            await ReturnFailureResponse(httpContext, ex);
        }
        catch (Exception ex)
        {
            HandleException(httpContext, ex);
            await ReturnFailureResponse(httpContext, ex);
        }
    }

    private void HandleException(HttpContext context, Exception exception)
    {
        var userContext = ApiHelper.GetUserContext(context);
        var (controllerName, actionName) = ApiHelper.GetApiEndPointInformation(context);

        _logger.LogError(
            exception,
            "Error occurred in Farmelo.API.Controllers.{ControllerName}-{MethodName} {Exception} | UserEmail:{Email}",
            controllerName,
            actionName,
            MethodHelper.FormatException(exception),
            userContext.Email);
    }

    private async Task ReturnFailureResponse(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var stackTrace = _stackTraceEnabled ? exception.StackTrace : string.Empty;
        await context.Response.WriteAsync(ApiHelper.ConstructHttpResponse(context.Response.StatusCode, exception.Message, stackTrace));
    }
}
