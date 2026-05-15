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

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, ServiceOperationResult<ProductDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IDapperExecutor _dapper;
    private readonly IRepository<Product> _products;

    public CreateProductCommandHandler(
        ICurrentUser currentUser,
        IDapperExecutor dapper,
        IRepository<Product> products)
    {
        _currentUser = currentUser;
        _dapper = dapper;
        _products = products;
    }

    public async Task<ServiceOperationResult<ProductDto>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId <= 0)
        {
            return ServiceOperationResult.CreateWithFailure<ProductDto>("Authenticated user context is required.");
        }

        var slug = TextNormalizer.NormalizeSlug(request.Request.Slug);
        var duplicate = await _dapper.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM Products WHERE Slug = @Slug",
            new { Slug = slug },
            ct: cancellationToken);

        if (duplicate > 0)
        {
            return ServiceOperationResult.CreateWithFailure<ProductDto>("A product with this slug already exists.");
        }

        var now = DateTime.UtcNow;
        var currentUserName = TextNormalizer.CurrentUserNameOrSystem(_currentUser.UserName);
        var product = new Product
        {
            Slug = slug,
            Name = request.Request.Name.Trim(),
            Description = request.Request.Description.Trim(),
            Weight = request.Request.Weight.Trim(),
            WeightGrams = request.Request.WeightGrams,
            CurrentPrice = request.Request.CurrentPrice,
            Currency = request.Request.Currency.Trim().ToUpperInvariant(),
            Ingredients = request.Request.Ingredients.Trim(),
            Nutrition = request.Request.Nutrition.Trim(),
            IsBestseller = request.Request.IsBestseller,
            IsActive = request.Request.IsActive,
            ImageUrl = TextNormalizer.NullIfWhiteSpace(request.Request.ImageUrl),
            AccentColor = TextNormalizer.NullIfWhiteSpace(request.Request.AccentColor),
            BackgroundColor = TextNormalizer.NullIfWhiteSpace(request.Request.BackgroundColor),
            CreatedBy = currentUserName,
            CreatedOn = now
        };

        product.PriceHistories.Add(new ProductPriceHistory
        {
            OldPrice = 0m,
            NewPrice = product.CurrentPrice,
            ChangedByUserId = _currentUser.UserId,
            ChangedOn = now,
            Reason = TextNormalizer.NullIfWhiteSpace(request.Request.PriceChangeReason) ?? "Initial product price",
            CreatedBy = currentUserName,
            CreatedOn = now
        });

        _products.Add(product);
        await _products.SaveChangesAsync(cancellationToken);

        return ServiceOperationResult.CreateWithSuccess(ProductDtoMapper.ToDto(product), "Product created.");
    }
}
