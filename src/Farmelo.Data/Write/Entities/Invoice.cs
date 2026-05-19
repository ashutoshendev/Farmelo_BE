using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class Invoice : BaseEntity
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceType { get; set; } = string.Empty;
    public int PartyId { get; set; }
    public int? B2BOrderId { get; set; }
    public int? B2CAssignmentId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PdfFileName { get; set; }
    public string? PdfPath { get; set; }
    public string EmailStatus { get; set; } = "Pending";
    public string? EmailError { get; set; }
    public DateTime? EmailSentOn { get; set; }

    public Party? Party { get; set; }
    public B2BOrder? B2BOrder { get; set; }
    public B2CAssignment? B2CAssignment { get; set; }
}
