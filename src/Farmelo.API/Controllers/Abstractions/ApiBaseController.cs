using Farmelo.API.ApiUtils;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.OperationResult;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers.Abstractions;

[Route("api/[controller]")]
[ApiController]
public abstract class ApiBaseController<T> : ControllerBase
{
    protected ApiBaseController(ILogger<T> logger, IMediator mediator)
    {
        Logger = logger;
        Mediator = mediator;
    }

    protected ILogger<T> Logger { get; }
    protected IMediator Mediator { get; }
    public string? ClientId => ApiHelper.GetHeaderValue<string>(AppConstants.ApiHeaders.ClientId, HttpContext);
    public string BearerToken => ApiHelper.GetAuthorizationToken(HttpContext);
    public string IpAddress => ApiHelper.GetIpAddress(HttpContext);

    protected string GetValidationErrors()
        => string.Join(
            '\n',
            ModelState.Values
                .Where(v => v.Errors.Count > 0)
                .SelectMany(v => v.Errors)
                .Select(v => v.ErrorMessage));

    protected BadRequestObjectResult ValidationFailureResult<TResponse>()
        => BadRequest(ServiceOperationResult.CreateWithFailure<TResponse>(GetValidationErrors()));

    protected BadRequestObjectResult FailureResult<TResponse>(string error)
        => BadRequest(ServiceOperationResult.CreateWithFailure<TResponse>(error));
}
