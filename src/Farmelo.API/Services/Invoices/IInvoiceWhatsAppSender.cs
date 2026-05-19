using Farmelo.Data.Write.Entities;

namespace Farmelo.API.Services.Invoices;

public interface IInvoiceWhatsAppSender
{
    Task<InvoiceWhatsAppResult> SendInvoiceAsync(
        Invoice invoice,
        string partyName,
        string? partyPhone,
        string pdfPath,
        CancellationToken cancellationToken);
}

public sealed record InvoiceWhatsAppResult(bool Success, string? Error, bool Skipped = false);
