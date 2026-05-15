using Farmelo.Data.Write.Entities;

namespace Farmelo.API.Services.Invoices;

public interface IInvoiceEmailSender
{
    Task<InvoiceEmailResult> SendInvoiceAsync(
        Invoice invoice,
        string partyName,
        string? partyEmail,
        string pdfPath,
        CancellationToken cancellationToken);
}

public sealed record InvoiceEmailResult(bool Success, string? Error);
