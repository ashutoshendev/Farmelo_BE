using Farmelo.Shared.DTO.Products;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Commands.Products;

public sealed record UpdateProductCommand(int ProductId, ProductUpdateRequestDto Request)
    : IRequest<ServiceOperationResult<ProductDto>>;
