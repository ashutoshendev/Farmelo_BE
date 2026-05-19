using Farmelo.Shared.Config;
using Farmelo.Shared.DTO.Notifications;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;

namespace Farmelo.API.Services.Notifications;

public sealed class InquiryNotificationService : IInquiryNotificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConfigurationOptions _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<InquiryNotificationService> _logger;

    public InquiryNotificationService(
        ConfigurationOptions config,
        HttpClient httpClient,
        ILogger<InquiryNotificationService> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<InquiryNotificationResponseDto> SendAsync(
        InquiryNotificationRequestDto request,
        CancellationToken cancellationToken)
    {
        var subject = Limit(FirstNotBlank(request.Subject, "Farmelo inquiry"), 180);
        var message = Limit(FirstNotBlank(request.Message, "A new Farmelo inquiry was submitted."), 4000);
        var body = BuildBody(request, message);
        var response = new InquiryNotificationResponseDto();

        var emailResult = await SendEmailAsync(subject, body, cancellationToken);
        response.EmailSent = emailResult.Success;
        response.EmailError = emailResult.Error;

        var whatsAppResult = await SendWhatsAppAsync(subject, body, cancellationToken);
        response.WhatsAppSent = whatsAppResult.Success;
        response.WhatsAppError = whatsAppResult.Error;

        return response;
    }

    private async Task<NotificationChannelResult> SendEmailAsync(
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var recipients = BuildEmailRecipients();
        if (recipients.Count == 0)
        {
            return new NotificationChannelResult(false, "No email recipients configured.");
        }

        if (string.IsNullOrWhiteSpace(_config.Email.Host) || string.IsNullOrWhiteSpace(_config.Email.FromEmail))
        {
            return new NotificationChannelResult(false, "SMTP host or from email is not configured.");
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_config.Email.FromEmail, _config.Email.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            foreach (var recipient in recipients)
            {
                message.To.Add(recipient);
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
            return new NotificationChannelResult(true, null);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Inquiry email failed.");
            return new NotificationChannelResult(false, ex.Message);
        }
    }

    private async Task<NotificationChannelResult> SendWhatsAppAsync(
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        if (!_config.WhatsApp.Enabled)
        {
            return new NotificationChannelResult(false, "WhatsApp is disabled.");
        }

        if (string.IsNullOrWhiteSpace(_config.WhatsApp.PhoneNumberId)
            || string.IsNullOrWhiteSpace(_config.WhatsApp.AccessToken))
        {
            return new NotificationChannelResult(false, "WhatsApp phone number id or access token is not configured.");
        }

        var recipients = BuildWhatsAppRecipients();
        if (recipients.Count == 0)
        {
            return new NotificationChannelResult(false, "No WhatsApp recipients configured.");
        }

        var text = $"{subject}\n\n{body}";
        var errors = new List<string>();
        foreach (var recipient in recipients)
        {
            using var request = CreateWhatsAppRequest();
            request.Content = JsonContent.Create(new
            {
                messaging_product = "whatsapp",
                to = recipient,
                type = "text",
                text = new
                {
                    preview_url = false,
                    body = text
                }
            }, options: JsonOptions);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var providerBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                errors.Add($"{recipient}: {ReadProviderError(providerBody)}");
            }
        }

        return errors.Count == 0
            ? new NotificationChannelResult(true, null)
            : new NotificationChannelResult(false, string.Join("; ", errors));
    }

    private List<string> BuildEmailRecipients()
    {
        var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recipient in _config.Notifications.EmailRecipients.Concat(_config.Invoices.Recipients))
        {
            if (!string.IsNullOrWhiteSpace(recipient))
            {
                recipients.Add(recipient.Trim());
            }
        }

        return recipients.ToList();
    }

    private List<string> BuildWhatsAppRecipients()
    {
        var recipients = new HashSet<string>(StringComparer.Ordinal);
        foreach (var recipient in _config.Notifications.WhatsAppRecipients.Concat(_config.WhatsApp.Recipients))
        {
            var phone = NormalizePhone(recipient);
            if (!string.IsNullOrWhiteSpace(phone))
            {
                recipients.Add(phone);
            }
        }

        return recipients.ToList();
    }

    private HttpRequestMessage CreateWhatsAppRequest()
    {
        var baseUrl = string.IsNullOrWhiteSpace(_config.WhatsApp.GraphApiBaseUrl)
            ? "https://graph.facebook.com"
            : _config.WhatsApp.GraphApiBaseUrl.TrimEnd('/');
        var apiVersion = string.IsNullOrWhiteSpace(_config.WhatsApp.ApiVersion)
            ? "v24.0"
            : _config.WhatsApp.ApiVersion.Trim('/');
        var phoneNumberId = _config.WhatsApp.PhoneNumberId.Trim();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/{apiVersion}/{phoneNumberId}/messages");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.WhatsApp.AccessToken);
        return request;
    }

    private string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal))
        {
            digits = digits[2..];
        }

        if (digits.StartsWith("0", StringComparison.Ordinal) && digits.Length == 11)
        {
            digits = digits[1..];
        }

        var countryCode = new string((_config.WhatsApp.DefaultCountryCode ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length == 10 && !string.IsNullOrWhiteSpace(countryCode))
        {
            digits = $"{countryCode}{digits}";
        }

        return digits.Length >= 8 ? digits : null;
    }

    private static string BuildBody(InquiryNotificationRequestDto request, string message)
    {
        var lines = new List<string>();
        AddLine(lines, "Source", request.Source);
        AddLine(lines, "Name", request.Name);
        AddLine(lines, "Email", request.Email);
        AddLine(lines, "Phone", request.Phone);
        lines.Add(string.Empty);
        lines.Add("Message:");
        lines.Add(message);
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddLine(List<string> lines, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            lines.Add($"{label}: {value.Trim()}");
        }
    }

    private static string FirstNotBlank(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static string Limit(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];

    private static string ReadProviderError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "Empty provider response.";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
            return body;
        }

        return body;
    }

    private sealed record NotificationChannelResult(bool Success, string? Error);
}
