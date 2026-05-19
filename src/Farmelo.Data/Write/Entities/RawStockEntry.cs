using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class RawStockEntry : BaseEntity
{
    public int Id { get; set; }
    public int? SellerPartyId { get; set; }
    public DateTime EntryDate { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal AvailableKg { get; set; }
    public decimal CostPerKg { get; set; }
    public string? SupplierName { get; set; }
    public string? Notes { get; set; }

    public Party? SellerParty { get; set; }
}
