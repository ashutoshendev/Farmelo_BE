using Farmelo.Business.Queries.Products;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Shared.DTO.Products;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Products;

public sealed class GetManageProductsQueryHandler
    : IRequestHandler<GetManageProductsQuery, ServiceOperationResult<IReadOnlyList<ProductDto>>>
{
    private readonly IDapperExecutor _dapper;

    public GetManageProductsQueryHandler(IDapperExecutor dapper)
    {
        _dapper = dapper;
    }

    public async Task<ServiceOperationResult<IReadOnlyList<ProductDto>>> Handle(
        GetManageProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _dapper.QueryAsync<ProductDto>(
            """
            SELECT Id, Slug, Name, Description, Weight, WeightGrams, CurrentPrice, Currency, Ingredients, Nutrition,
                   IsBestseller, IsActive, ImageUrl, AccentColor, BackgroundColor, CreatedOn, ModifiedOn
            FROM Products
            ORDER BY IsActive DESC, IsBestseller DESC, Name
            """,
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(products);
    }
}
