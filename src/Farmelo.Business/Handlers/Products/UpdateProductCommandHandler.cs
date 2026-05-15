using Farmelo.Business.Commands.Products;
using Farmelo.Business.Mapping;
using Farmelo.Business.Support;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.Entities;
using Farmelo.Data.Write.IRepository;
using Farmelo.Shared.DTO.Products;
using Farmelo.Shared.OperationResult;
using MediatR;

namespace Farmelo.Business.Handlers.Products;

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, ServiceOperationResult<ProductDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IDapperExecutor _dapper;
    private readonly IRepository<Product> _products;
    private readonly IRepository<ProductPriceHistory> _priceHistories;

    public UpdateProductCommandHandler(
        ICurrentUser currentUser,
        IDapperExecutor dapper,
        IRepository<Product> products,
        IRepository<ProductPriceHistory> priceHistories)
    {
        _currentUser = currentUser;
        _dapper = dapper;
        _products = products;
        _priceHistories = priceHistories;
    }

    public async Task<ServiceOperationResult<ProductDto>> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId <= 0)
        {
            return ServiceOperationResult.CreateWithFailure<ProductDto>("Authenticated user context is required.");
        }

        var product = await _products.FindAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return ServiceOperationResult.CreateWithFailure<ProductDto>("Product was not found.");
        }

        var slug = TextNormalizer.NormalizeSlug(request.Request.Slug);
        var duplicate = await _dapper.QuerySingleAsync<int>(
            """
            SELECT COUNT(1)
            FROM Products
            WHERE Id <> @ProductId AND Slug = @Slug
            """,
            new { request.ProductId, Slug = slug },
            ct: cancellationToken);

        if (duplicate > 0)
        {
            return ServiceOperationResult.CreateWithFailure<ProductDto>("A product with this slug already exists.");
        }

        var now = DateTime.UtcNow;
        var currentUserName = TextNormalizer.CurrentUserNameOrSystem(_currentUser.UserName);
        var oldPrice = product.CurrentPrice;
        var priceChanged = oldPrice != request.Request.CurrentPrice;

        product.Slug = slug;
        product.Name = request.Request.Name.Trim();
        product.Description = request.Request.Description.Trim();
        product.Weight = request.Request.Weight.Trim();
        product.WeightGrams = request.Request.WeightGrams;
        product.CurrentPrice = request.Request.CurrentPrice;
        product.Currency = request.Request.Currency.Trim().ToUpperInvariant();
        product.Ingredients = request.Request.Ingredients.Trim();
        product.Nutrition = request.Request.Nutrition.Trim();
        product.IsBestseller = request.Request.IsBestseller;
        product.IsActive = request.Request.IsActive;
        product.ImageUrl = TextNormalizer.NullIfWhiteSpace(request.Request.ImageUrl);
        product.AccentColor = TextNormalizer.NullIfWhiteSpace(request.Request.AccentColor);
        product.BackgroundColor = TextNormalizer.NullIfWhiteSpace(request.Request.BackgroundColor);
        product.ModifiedBy = currentUserName;
        product.ModifiedOn = now;

        if (priceChanged)
        {
            _priceHistories.Add(new ProductPriceHistory
            {
                ProductId = product.Id,
                OldPrice = oldPrice,
                NewPrice = product.CurrentPrice,
                ChangedByUserId = _currentUser.UserId,
                ChangedOn = now,
                Reason = TextNormalizer.NullIfWhiteSpace(request.Request.PriceChangeReason),
                CreatedBy = currentUserName,
                CreatedOn = now
            });
        }

        _products.Update(product);
        await _products.SaveChangesAsync(cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(ProductDtoMapper.ToDto(product), "Product updated.");
    }
}
