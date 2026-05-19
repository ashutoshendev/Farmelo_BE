using Farmelo.Shared.DTO.Business;

namespace Farmelo.Business.Services.Invoices;

public interface IInvoiceService
{
    Task<InvoiceDto?> CreateForB2BOrderAsync(int orderId, CancellationToken cancellationToken);
    Task<InvoiceDto?> CreateForB2CAssignmentAsync(int assignmentId, CancellationToken cancellationToken);
    Task<InvoiceDto?> RegeneratePdfAsync(long invoiceId, CancellationToken cancellationToken);
    Task<InvoiceDto?> SendNotificationsAsync(long invoiceId, CancellationToken cancellationToken);
}
