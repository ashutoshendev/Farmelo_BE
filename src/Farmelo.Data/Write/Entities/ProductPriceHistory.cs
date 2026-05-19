using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class ProductPriceHistory : BaseEntity
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public int ChangedByUserId { get; set; }
    public DateTime ChangedOn { get; set; }
    public string? Reason { get; set; }

    public Product? Product { get; set; }
    public UserAccount? ChangedByUser { get; set; }
}
