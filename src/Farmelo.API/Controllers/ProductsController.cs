using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Farmelo.API.Controllers;

[Route("api/products")]
public sealed class ProductsController : ApiBaseController<ProductsController>
{
    public ProductsController(ILogger<ProductsController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPublicProductsQuery(), ct));
}
