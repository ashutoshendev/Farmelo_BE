using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class Payment : BaseEntity
{
    public int Id { get; set; }
    public int PartyId { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public int? B2BOrderId { get; set; }
    public int? B2CAssignmentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMode { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    public Party? Party { get; set; }
    public B2BOrder? B2BOrder { get; set; }
    public B2CAssignment? B2CAssignment { get; set; }
}
