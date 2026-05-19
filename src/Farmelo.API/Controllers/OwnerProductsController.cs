using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Commands.Products;
using Farmelo.Business.Queries.Products;
using Farmelo.Shared.CommonHelper;
using Farmelo.Shared.DTO.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Authorize(Roles = AppConstants.Roles.Admin + "," + AppConstants.Roles.Owner)]
[Route("api/owner/products")]
public sealed class OwnerProductsController : ApiBaseController<OwnerProductsController>
{
    public OwnerProductsController(ILogger<OwnerProductsController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken ct)
        => Ok(await Mediator.Send(new GetManageProductsQuery(), ct));

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<ProductDto>();
        }

        var result = await Mediator.Send(new CreateProductCommand(request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{productId:int}")]
    public async Task<IActionResult> UpdateProduct(
        int productId,
        [FromBody] ProductUpdateRequestDto? request,
        CancellationToken ct)
    {
        if (productId <= 0)
        {
            return InvalidRouteIdResult<ProductDto>(nameof(productId));
        }

        if (request == null)
        {
            return MissingBodyResult<ProductDto>();
        }

        var result = await Mediator.Send(new UpdateProductCommand(productId, request), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
