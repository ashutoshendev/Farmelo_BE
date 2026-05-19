using Farmelo.Business.Queries.Products;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Shared.DTO.Products;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Products;

public sealed class GetPublicProductsQueryHandler
    : IRequestHandler<GetPublicProductsQuery, ServiceOperationResult<IReadOnlyList<ProductDto>>>
{
    private readonly IDapperExecutor _dapper;

    public GetPublicProductsQueryHandler(IDapperExecutor dapper)
    {
        _dapper = dapper;
    }

    public async Task<ServiceOperationResult<IReadOnlyList<ProductDto>>> Handle(
        GetPublicProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _dapper.QueryAsync<ProductDto>(
            """
            SELECT Id, Slug, Name, Description, Weight, WeightGrams, CurrentPrice, Currency, Ingredients, Nutrition,
                   IsBestseller, IsActive, ImageUrl, AccentColor, BackgroundColor, CreatedOn, ModifiedOn
            FROM Products
            WHERE IsActive = 1
            ORDER BY IsBestseller DESC, Name
            """,
            ct: cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(products);
    }
}
