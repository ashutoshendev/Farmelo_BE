using Farmelo.API.Controllers.Abstractions;
using Farmelo.Business.Services.Invoices;
using Farmelo.Data.Write.EFContext;
using Farmelo.Shared.CommonHelper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.API.Controllers;

[Authorize(Roles = AppConstants.Roles.Admin + "," + AppConstants.Roles.Owner)]
[Route("api/invoices")]
public sealed class InvoicesController : ApiBaseController<InvoicesController>
{
    private readonly FarmeloDbContext _dbContext;
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(
        ILogger<InvoicesController> logger,
        IMediator mediator,
        FarmeloDbContext dbContext,
        IInvoiceService invoiceService)
        : base(logger, mediator)
    {
        _dbContext = dbContext;
        _invoiceService = invoiceService;
    }

    [HttpGet("{invoiceId:long}/download")]
    public async Task<IActionResult> Download(long invoiceId, CancellationToken ct)
    {
        if (invoiceId <= 0)
        {
            return BadRequest("invoiceId must be greater than zero.");
        }

        await _invoiceService.RegeneratePdfAsync(invoiceId, ct);

        var invoice = await _dbContext.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == invoiceId, ct);
        if (invoice == null || string.IsNullOrWhiteSpace(invoice.PdfPath) || !System.IO.File.Exists(invoice.PdfPath))
        {
            return NotFound("Invoice PDF was not found.");
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(invoice.PdfPath, ct);
        return File(bytes, "application/pdf", invoice.PdfFileName ?? $"{invoice.InvoiceNumber.Replace('/', '-')}.pdf");
    }
}
