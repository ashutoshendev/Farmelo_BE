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

        return ServiceOperationResult.CreateWithSuccess(products.Count > 0 ? products : StaticFarmeloCatalog);
    }

    private static readonly IReadOnlyList<ProductDto> StaticFarmeloCatalog =
    [
        CreateStaticProduct(1, "cream-and-onion", "Cream and Onion", "Smooth onion with herby creaminess.", 160m, true, "/products/cream-and-onion.png", "#326f2b", "#edf6dc"),
        CreateStaticProduct(2, "chatpata-masala", "Chatpata Masala", "Bold masala warmth with a snackable tang.", 160m, true, "/products/chatpata-masala.png", "#d2381d", "#fff1e8"),
        CreateStaticProduct(3, "fiery-peri-peri", "Fiery Peri Peri", "Fiery chilli, garlic and tang.", 160m, true, "/products/fiery-peri-peri.png", "#9f1d23", "#fff0ed"),
        CreateStaticProduct(4, "minty-pudina", "Minty Pudina", "Cooling pudina lift with a clean herb finish.", 160m, false, "/products/minty-pudina.png", "#87951f", "#f3f7dc"),
        CreateStaticProduct(5, "salt-and-pepper", "Salt and Pepper", "Classic seasoning with a sharp pepper finish.", 160m, false, "/products/salt-and-pepper.png", "#25231d", "#f4ede0")
    ];

    private static ProductDto CreateStaticProduct(
        int id,
        string slug,
        string name,
        string description,
        decimal price,
        bool isBestseller,
        string imageUrl,
        string accentColor,
        string backgroundColor)
        => new()
        {
            Id = id,
            Slug = slug,
            Name = name,
            Description = description,
            Weight = "100g",
            WeightGrams = 100,
            CurrentPrice = price,
            Currency = "INR",
            Ingredients = $"Roasted makhana, {name.ToLowerInvariant()} seasoning.",
            Nutrition = "Roasted not fried, gluten-free and light for everyday snacking.",
            IsBestseller = isBestseller,
            IsActive = true,
            ImageUrl = imageUrl,
            AccentColor = accentColor,
            BackgroundColor = backgroundColor
        };
}
