namespace Farmelo.Shared.DTO.Products;

public sealed class ProductUpdateRequestDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public int WeightGrams { get; set; }
    public decimal CurrentPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public string Ingredients { get; set; } = string.Empty;
    public string Nutrition { get; set; } = string.Empty;
    public bool IsBestseller { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string? AccentColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? PriceChangeReason { get; set; }
}
