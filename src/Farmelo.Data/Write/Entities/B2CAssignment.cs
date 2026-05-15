using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class B2CAssignment : BaseEntity
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public int ProductId { get; set; }
    public DateTime AssignmentDate { get; set; }
    public DateTime DueDate { get; set; }
    public int Quantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Margin { get; set; }
    public string? Notes { get; set; }

    public Party? Party { get; set; }
    public Product? Product { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
