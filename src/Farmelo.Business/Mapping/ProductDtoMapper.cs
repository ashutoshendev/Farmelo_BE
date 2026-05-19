using Farmelo.Data.Write.Entities;
using Farmelo.Shared.DTO.Products;

namespace Farmelo.Business.Mapping;

internal static class ProductDtoMapper
{
    public static ProductDto ToDto(Product product)
        => new()
        {
            Id = product.Id,
            Slug = product.Slug,
            Name = product.Name,
            Description = product.Description,
            Weight = product.Weight,
            WeightGrams = product.WeightGrams,
            CurrentPrice = product.CurrentPrice,
            Currency = product.Currency,
            Ingredients = product.Ingredients,
            Nutrition = product.Nutrition,
            IsBestseller = product.IsBestseller,
            IsActive = product.IsActive,
            ImageUrl = product.ImageUrl,
            AccentColor = product.AccentColor,
            BackgroundColor = product.BackgroundColor,
            CreatedOn = product.CreatedOn,
            ModifiedOn = product.ModifiedOn
        };
}
