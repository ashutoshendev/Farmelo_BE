using Farmelo.Shared.DTO.Products;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Queries.Products;

public sealed record GetManageProductsQuery : IRequest<ServiceOperationResult<IReadOnlyList<ProductDto>>>;
