using Farmelo.Shared.OperationResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Farmelo.API.ActionFilters;

public sealed class ModelStateFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var errors = string.Join(
            '\n',
            context.ModelState.Values
                .Where(v => v.Errors.Count > 0)
                .SelectMany(v => v.Errors)
                .Select(v => v.ErrorMessage));

        context.Result = new BadRequestObjectResult(ServiceOperationResult.CreateWithFailure<object>(errors));
    }
}
