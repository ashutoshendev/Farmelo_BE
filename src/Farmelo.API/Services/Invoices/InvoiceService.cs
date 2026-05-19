using Farmelo.Business.Services.Invoices;
using Farmelo.Data.Write.EFContext;
using Farmelo.Data.Write.Entities;
using Farmelo.Shared.Config;
using Farmelo.Shared.DTO.Business;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.Globalization;

namespace Farmelo.API.Services.Invoices;

public sealed class InvoiceService : IInvoiceService
{
    private static readonly CultureInfo IndiaCulture = CultureInfo.GetCultureInfo("en-IN");
    private readonly FarmeloDbContext _dbContext;
    private readonly ConfigurationOptions _config;
    private readonly IInvoiceEmailSender _emailSender;
    private readonly IInvoiceWhatsAppSender _whatsAppSender;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        FarmeloDbContext dbContext,
        ConfigurationOptions config,
        IInvoiceEmailSender emailSender,
        IInvoiceWhatsAppSender whatsAppSender,
        IWebHostEnvironment environment,
        ILogger<InvoiceService> logger)
    {
        _dbContext = dbContext;
        _config = config;
        _emailSender = emailSender;
        _whatsAppSender = whatsAppSender;
        _environment = environment;
        _logger = logger;
    }

    public async Task<InvoiceDto?> CreateForB2BOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await ExistingInvoiceAsync("B2B", orderId, cancellationToken);
            if (existing != null)
            {
                return existing;
            }

            var order = await _dbContext.B2BOrders.AsNoTracking()
                .Where(x => x.Id == orderId)
                .Select(x => new
                {
                    Order = x,
                    PartyName = x.Party!.Name,
                    x.Party.ContactName,
                    PartyEmail = x.Party.Email,
                    x.Party.Location,
                    x.Party.Phone
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (order == null)
            {
                return null;
            }

            var invoice = await CreateInvoiceRowAsync("B2B", order.Order.PartyId, orderId, null, order.Order.OrderDate, order.Order.TotalValue, cancellationToken);
            var item = new InvoiceItem(
                $"Bulk Makhana - {order.Order.SuttaGrade}",
                $"{order.Order.QuantityKg:N3} KG",
                order.Order.PricePerKg,
                order.Order.TotalValue);
            var party = new PartyInvoiceInfo(order.PartyName, order.ContactName, order.PartyEmail, order.Location, order.Phone);
            await GenerateAndSendAsync(invoice, party, [item], true, cancellationToken);
            return ToInvoiceDto(invoice, order.PartyName);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Invoice generation failed for B2B order {OrderId}", orderId);
            return null;
        }
    }

    public async Task<InvoiceDto?> CreateForB2CAssignmentAsync(int assignmentId, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await ExistingInvoiceAsync("B2C", assignmentId, cancellationToken);
            if (existing != null)
            {
                return existing;
            }

            var assignment = await _dbContext.B2CAssignments.AsNoTracking()
                .Where(x => x.Id == assignmentId)
                .Select(x => new
                {
                    Assignment = x,
                    PartyName = x.Party!.Name,
                    x.Party.ContactName,
                    PartyEmail = x.Party.Email,
                    x.Party.Location,
                    x.Party.Phone,
                    ProductName = x.Product!.Name
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (assignment == null)
            {
                return null;
            }

            var invoice = await CreateInvoiceRowAsync(
                "B2C",
                assignment.Assignment.PartyId,
                null,
                assignmentId,
                assignment.Assignment.AssignmentDate,
                assignment.Assignment.TotalValue,
                cancellationToken);
            var item = new InvoiceItem(
                $"{assignment.ProductName} - Box Stock Assignment",
                $"{assignment.Assignment.Quantity:N0} Boxes",
                assignment.Assignment.UnitPrice,
                assignment.Assignment.TotalValue);
            var party = new PartyInvoiceInfo(
                assignment.PartyName,
                assignment.ContactName,
                assignment.PartyEmail,
                assignment.Location,
                assignment.Phone);
            await GenerateAndSendAsync(invoice, party, [item], true, cancellationToken);
            return ToInvoiceDto(invoice, assignment.PartyName);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Invoice generation failed for B2C assignment {AssignmentId}", assignmentId);
            return null;
        }
    }

    public Task<InvoiceDto?> RegeneratePdfAsync(long invoiceId, CancellationToken cancellationToken)
        => RegenerateAsync(invoiceId, false, cancellationToken);

    public Task<InvoiceDto?> SendNotificationsAsync(long invoiceId, CancellationToken cancellationToken)
        => RegenerateAsync(invoiceId, true, cancellationToken);

    private async Task<InvoiceDto?> RegenerateAsync(long invoiceId, bool sendNotifications, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await _dbContext.Invoices.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);
            if (invoice == null)
            {
                return null;
            }

            if (invoice.InvoiceType == "B2B" && invoice.B2BOrderId.HasValue)
            {
                var order = await _dbContext.B2BOrders.AsNoTracking()
                    .Where(x => x.Id == invoice.B2BOrderId.Value)
                    .Select(x => new
                    {
                        Order = x,
                        PartyName = x.Party!.Name,
                        x.Party.ContactName,
                        PartyEmail = x.Party.Email,
                        x.Party.Location,
                        x.Party.Phone
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                if (order == null)
                {
                    return null;
                }

                var item = new InvoiceItem(
                    $"Bulk Makhana - {order.Order.SuttaGrade}",
                    $"{order.Order.QuantityKg:N3} KG",
                    order.Order.PricePerKg,
                    order.Order.TotalValue);
                var party = new PartyInvoiceInfo(order.PartyName, order.ContactName, order.PartyEmail, order.Location, order.Phone);
                await GenerateAndSendAsync(invoice, party, [item], sendNotifications, cancellationToken);
                return ToInvoiceDto(invoice, order.PartyName);
            }

            if (invoice.InvoiceType == "B2C" && invoice.B2CAssignmentId.HasValue)
            {
                var assignment = await _dbContext.B2CAssignments.AsNoTracking()
                    .Where(x => x.Id == invoice.B2CAssignmentId.Value)
                    .Select(x => new
                    {
                        Assignment = x,
                        PartyName = x.Party!.Name,
                        x.Party.ContactName,
                        PartyEmail = x.Party.Email,
                        x.Party.Location,
                        x.Party.Phone,
                        ProductName = x.Product!.Name
                    })
                    .FirstOrDefaultAsync(cancellationToken);
                if (assignment == null)
                {
                    return null;
                }

                var item = new InvoiceItem(
                    $"{assignment.ProductName} - Box Stock Assignment",
                    $"{assignment.Assignment.Quantity:N0} Boxes",
                    assignment.Assignment.UnitPrice,
                    assignment.Assignment.TotalValue);
                var party = new PartyInvoiceInfo(
                    assignment.PartyName,
                    assignment.ContactName,
                    assignment.PartyEmail,
                    assignment.Location,
                    assignment.Phone);
                await GenerateAndSendAsync(invoice, party, [item], sendNotifications, cancellationToken);
                return ToInvoiceDto(invoice, assignment.PartyName);
            }

            return null;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Invoice regeneration or notification failed for invoice {InvoiceId}", invoiceId);
            return null;
        }
    }

    private async Task<InvoiceDto?> ExistingInvoiceAsync(string invoiceType, int referenceId, CancellationToken cancellationToken)
    {
        var query = _dbContext.Invoices.AsNoTracking().Where(x => x.InvoiceType == invoiceType);
        query = invoiceType == "B2B"
            ? query.Where(x => x.B2BOrderId == referenceId)
            : query.Where(x => x.B2CAssignmentId == referenceId);

        var invoice = await query
            .Select(x => new { Invoice = x, PartyName = x.Party!.Name })
            .FirstOrDefaultAsync(cancellationToken);

        return invoice == null ? null : ToInvoiceDto(invoice.Invoice, invoice.PartyName);
    }

    private async Task<Invoice> CreateInvoiceRowAsync(
        string invoiceType,
        int partyId,
        int? b2BOrderId,
        int? b2CAssignmentId,
        DateTime invoiceDate,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = await NextInvoiceNumberAsync(cancellationToken),
            InvoiceType = invoiceType,
            PartyId = partyId,
            B2BOrderId = b2BOrderId,
            B2CAssignmentId = b2CAssignmentId,
            InvoiceDate = invoiceDate,
            TotalAmount = totalAmount,
            EmailStatus = "Pending",
            CreatedBy = "SYSTEM",
            CreatedOn = DateTime.UtcNow
        };

        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    private async Task<string> NextInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow.AddHours(5.5);
        var startYear = now.Month >= 4 ? now.Year : now.Year - 1;
        var prefixText = string.IsNullOrWhiteSpace(_config.Invoices.NumberPrefix)
            ? "KAJ"
            : _config.Invoices.NumberPrefix.Trim().ToUpperInvariant();
        var prefix = $"{prefixText}/{startYear}-{(startYear + 1) % 100:00}/";
        var existingCount = await _dbContext.Invoices.CountAsync(x => x.InvoiceNumber.StartsWith(prefix), cancellationToken);
        return $"{prefix}{existingCount + 1:0000}";
    }

    private async Task GenerateAndSendAsync(
        Invoice invoice,
        PartyInvoiceInfo party,
        IReadOnlyList<InvoiceItem> items,
        bool sendEmail,
        CancellationToken cancellationToken)
    {
        var outputPath = ResolveOutputPath();
        Directory.CreateDirectory(outputPath);
        invoice.PdfFileName = $"{invoice.InvoiceNumber.Replace('/', '-')}.pdf";
        invoice.PdfPath = Path.Combine(outputPath, invoice.PdfFileName);

        GeneratePdf(invoice, party, items, invoice.PdfPath);

        if (sendEmail)
        {
            var emailResult = await _emailSender.SendInvoiceAsync(invoice, party.Name, party.Email, invoice.PdfPath, cancellationToken);
            invoice.EmailStatus = emailResult.Success ? "Sent" : "Failed";
            invoice.EmailError = emailResult.Error;
            invoice.EmailSentOn = emailResult.Success ? DateTime.UtcNow : null;

            var whatsAppResult = await _whatsAppSender.SendInvoiceAsync(invoice, party.Name, party.Phone, invoice.PdfPath, cancellationToken);
            if (!whatsAppResult.Skipped && !whatsAppResult.Success)
            {
                _logger.LogWarning(
                    "Invoice WhatsApp notification failed for {InvoiceNumber}: {Error}",
                    invoice.InvoiceNumber,
                    whatsAppResult.Error);
            }
        }

        invoice.ModifiedBy = "SYSTEM";
        invoice.ModifiedOn = DateTime.UtcNow;
        _dbContext.Invoices.Update(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void GeneratePdf(
        Invoice invoice,
        PartyInvoiceInfo party,
        IReadOnlyList<InvoiceItem> items,
        string outputPath)
    {
        var templatePath = ResolveTemplatePath();
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("Invoice template was not found.", templatePath);
        }

        using var template = PdfReader.Open(templatePath, PdfDocumentOpenMode.Import);
        using var document = new PdfDocument();
        for (var i = 0; i < template.PageCount; i++)
        {
            document.AddPage(template.Pages[i]);
        }

        var page = document.Pages[0];
        using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
        var regular = new XFont("Arial", 9, XFontStyle.Regular);
        var bold = new XFont("Arial", 10, XFontStyle.Bold);
        var small = new XFont("Arial", 8, XFontStyle.Regular);

        Write(gfx, bold, _config.Invoices.FirmName, 38, 48, 220, 16);
        Write(gfx, bold, invoice.InvoiceNumber, 385, 78, 132, 14, XStringFormats.TopLeft);
        Write(gfx, regular, FormatDate(invoice.InvoiceDate), 385, 119, 132, 14, XStringFormats.TopLeft);

        var addressLines = BuildAddressLines(party);
        WriteMultiline(gfx, small, addressLines, 42, 150, 250, 12);
        WriteMultiline(gfx, small, addressLines, 42, 230, 250, 12);

        var y = 368d;
        var serial = 1;
        foreach (var item in items)
        {
            Write(gfx, regular, serial.ToString(IndiaCulture), 56, y, 24, 14);
            Write(gfx, regular, item.Label, 88, y, 210, 14);
            Write(gfx, regular, item.Quantity, 300, y, 70, 14);
            Write(gfx, regular, FormatCurrency(item.Rate), 382, y, 70, 14, XStringFormats.TopRight);
            Write(gfx, regular, FormatCurrency(item.Total), 470, y, 70, 14, XStringFormats.TopRight);
            y -= 22;
            serial++;
        }

        Write(gfx, bold, FormatCurrency(invoice.TotalAmount), 470, 678, 70, 16, XStringFormats.TopRight);
        Write(gfx, regular, "Generated by Farmelo Business Management System", 56, 744, 300, 12);
        document.Save(outputPath);
    }

    private string ResolveTemplatePath()
    {
        if (!string.IsNullOrWhiteSpace(_config.Invoices.TemplatePath))
        {
            var configured = Path.IsPathRooted(_config.Invoices.TemplatePath)
                ? _config.Invoices.TemplatePath
                : Path.Combine(_environment.ContentRootPath, _config.Invoices.TemplatePath);
            if (File.Exists(configured))
            {
                return configured;
            }
        }

        return Path.Combine(_environment.ContentRootPath, "Templates", "Invoices", "INV011.pdf");
    }

    private string ResolveOutputPath()
    {
        if (string.IsNullOrWhiteSpace(_config.Invoices.OutputPath))
        {
            return Path.Combine(_environment.ContentRootPath, "Generated", "Invoices");
        }

        return Path.IsPathRooted(_config.Invoices.OutputPath)
            ? _config.Invoices.OutputPath
            : Path.Combine(_environment.ContentRootPath, _config.Invoices.OutputPath);
    }

    private InvoiceDto ToInvoiceDto(Invoice invoice, string partyName)
        => new()
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceType = invoice.InvoiceType,
            PartyId = invoice.PartyId,
            PartyName = partyName,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = invoice.TotalAmount,
            EmailStatus = invoice.EmailStatus,
            EmailError = invoice.EmailError,
            EmailSentOn = invoice.EmailSentOn,
            DownloadUrl = $"{ResolvedDownloadBaseUrl()}/{invoice.Id}/download"
        };

    private string ResolvedDownloadBaseUrl()
        => string.IsNullOrWhiteSpace(_config.Invoices.DownloadBaseUrl)
            ? "/api/invoices"
            : _config.Invoices.DownloadBaseUrl.TrimEnd('/');

    private static void Write(
        XGraphics gfx,
        XFont font,
        string text,
        double x,
        double y,
        double width,
        double height,
        XStringFormat? format = null)
        => gfx.DrawString(text, font, XBrushes.Black, new XRect(x, y, width, height), format ?? XStringFormats.TopLeft);

    private static void WriteMultiline(
        XGraphics gfx,
        XFont font,
        IReadOnlyList<string> lines,
        double x,
        double y,
        double width,
        double lineHeight)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            Write(gfx, font, lines[i], x, y + i * lineHeight, width, lineHeight);
        }
    }

    private static IReadOnlyList<string> BuildAddressLines(PartyInvoiceInfo party)
    {
        var lines = new List<string> { party.Name };
        AddIfPresent(lines, party.ContactName);
        AddIfPresent(lines, party.Location);
        AddIfPresent(lines, party.Phone);
        AddIfPresent(lines, party.Email);
        return lines;
    }

    private static void AddIfPresent(List<string> lines, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            lines.Add(value.Trim());
        }
    }

    private static string FormatDate(DateTime value)
        => value.ToString("dd-MM-yyyy", IndiaCulture);

    private static string FormatCurrency(decimal value)
        => string.Format(IndiaCulture, "{0:C2}", value);

    private sealed record PartyInvoiceInfo(string Name, string? ContactName, string? Email, string? Location, string? Phone);

    private sealed record InvoiceItem(string Label, string Quantity, decimal Rate, decimal Total);
}
