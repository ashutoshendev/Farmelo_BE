using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class Product : BaseEntity
{
    public int Id { get; set; }
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

    public ICollection<ProductPriceHistory> PriceHistories { get; set; } = new List<ProductPriceHistory>();
    public ICollection<BoxStockMovement> BoxStockMovements { get; set; } = new List<BoxStockMovement>();
    public ICollection<B2CAssignment> B2CAssignments { get; set; } = new List<B2CAssignment>();
}
