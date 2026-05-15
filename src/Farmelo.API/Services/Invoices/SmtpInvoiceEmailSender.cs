using Farmelo.Data.Write.Entities;
using Farmelo.Shared.Config;
using System.Net;
using System.Net.Mail;

namespace Farmelo.API.Services.Invoices;

public sealed class SmtpInvoiceEmailSender : IInvoiceEmailSender
{
    private readonly ConfigurationOptions _config;
    private readonly ILogger<SmtpInvoiceEmailSender> _logger;

    public SmtpInvoiceEmailSender(ConfigurationOptions config, ILogger<SmtpInvoiceEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<InvoiceEmailResult> SendInvoiceAsync(
        Invoice invoice,
        string partyName,
        string? partyEmail,
        string pdfPath,
        CancellationToken cancellationToken)
    {
        var recipients = BuildRecipients(partyEmail);
        if (recipients.Count == 0)
        {
            return new InvoiceEmailResult(false, "No invoice recipients configured.");
        }

        if (string.IsNullOrWhiteSpace(_config.Email.Host) || string.IsNullOrWhiteSpace(_config.Email.FromEmail))
        {
            return new InvoiceEmailResult(false, "SMTP host or from email is not configured.");
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_config.Email.FromEmail, _config.Email.FromName),
                Subject = $"Invoice {invoice.InvoiceNumber} - Kaj International",
                Body = $"Dear {partyName},\n\nPlease find attached invoice {invoice.InvoiceNumber}.\n\nRegards,\nKaj International",
                IsBodyHtml = false
            };

            foreach (var recipient in recipients)
            {
                message.To.Add(recipient);
            }

            if (File.Exists(pdfPath))
            {
                message.Attachments.Add(new Attachment(pdfPath, "application/pdf"));
            }

            using var client = new SmtpClient(_config.Email.Host, _config.Email.Port)
            {
                EnableSsl = _config.Email.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(_config.Email.UserName))
            {
                client.Credentials = new NetworkCredential(_config.Email.UserName, _config.Email.Password);
            }

            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message);
            return new InvoiceEmailResult(true, null);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or IOException)
        {
            _logger.LogWarning(ex, "Invoice email failed for {InvoiceNumber}", invoice.InvoiceNumber);
            return new InvoiceEmailResult(false, ex.Message);
        }
    }

    private List<string> BuildRecipients(string? partyEmail)
    {
        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recipient in _config.Invoices.Recipients.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            recipients.Add(recipient.Trim());
        }

        if (!string.IsNullOrWhiteSpace(partyEmail))
        {
            recipients.Add(partyEmail.Trim());
        }

        return recipients.ToList();
    }
}
