using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class B2BOrder : BaseEntity
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime DueDate { get; set; }
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal TotalValue { get; set; }
    public decimal CostPerKg { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Margin { get; set; }
    public string? Notes { get; set; }

    public Party? Party { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
