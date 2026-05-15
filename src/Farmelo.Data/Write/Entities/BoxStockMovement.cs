using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class BoxStockMovement : BaseEntity
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public DateTime MovementDate { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public string SuttaGrade { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }

    public Product? Product { get; set; }
}
