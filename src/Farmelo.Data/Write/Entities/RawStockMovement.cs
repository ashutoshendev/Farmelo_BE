using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class RawStockMovement : BaseEntity
{
    public long Id { get; set; }
    public DateTime MovementDate { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string SuttaGrade { get; set; } = string.Empty;
    public decimal QuantityKg { get; set; }
    public decimal CostPerKg { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
}
