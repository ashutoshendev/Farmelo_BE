using Farmelo.Data.Write.Entities;
using Farmelo.Shared.Config;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Farmelo.API.Services.Invoices;

public sealed class MetaWhatsAppInvoiceSender : IInvoiceWhatsAppSender
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConfigurationOptions _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetaWhatsAppInvoiceSender> _logger;

    public MetaWhatsAppInvoiceSender(
        ConfigurationOptions config,
        HttpClient httpClient,
        ILogger<MetaWhatsAppInvoiceSender> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<InvoiceWhatsAppResult> SendInvoiceAsync(
        Invoice invoice,
        string partyName,
        string? partyPhone,
        string pdfPath,
        CancellationToken cancellationToken)
    {
        if (!_config.WhatsApp.Enabled)
        {
            return new InvoiceWhatsAppResult(true, null, true);
        }

        var recipients = BuildRecipients(partyPhone);
        if (recipients.Count == 0)
        {
            return new InvoiceWhatsAppResult(false, "No WhatsApp recipients configured.");
        }

        if (string.IsNullOrWhiteSpace(_config.WhatsApp.PhoneNumberId)
            || string.IsNullOrWhiteSpace(_config.WhatsApp.AccessToken))
        {
            return new InvoiceWhatsAppResult(false, "WhatsApp phone number id or access token is not configured.");
        }

        try
        {
            var caption = BuildCaption(invoice, partyName);
            var mediaId = File.Exists(pdfPath)
                ? await UploadMediaAsync(pdfPath, cancellationToken)
                : null;

            var errors = new List<string>();
            foreach (var recipient in recipients)
            {
                var result = mediaId == null
                    ? await SendTextAsync(recipient, caption, cancellationToken)
                    : await SendDocumentAsync(recipient, mediaId, invoice.PdfFileName ?? $"{invoice.InvoiceNumber}.pdf", caption, cancellationToken);

                if (!result.Success)
                {
                    errors.Add($"{recipient}: {result.Error}");
                }
            }

            return errors.Count == 0
                ? new InvoiceWhatsAppResult(true, null)
                : new InvoiceWhatsAppResult(false, string.Join("; ", errors));
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or IOException or JsonException)
        {
            _logger.LogWarning(ex, "Invoice WhatsApp send failed for {InvoiceNumber}", invoice.InvoiceNumber);
            return new InvoiceWhatsAppResult(false, ex.Message);
        }
    }

    private async Task<string?> UploadMediaAsync(string pdfPath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(pdfPath);
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        form.Add(new StringContent("whatsapp"), "messaging_product");
        form.Add(new StringContent("application/pdf"), "type");
        form.Add(fileContent, "file", Path.GetFileName(pdfPath));

        using var request = CreateRequest(HttpMethod.Post, "media");
        request.Content = form;
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"WhatsApp media upload failed: {ReadProviderError(body)}");
        }

        var result = JsonSerializer.Deserialize<MediaUploadResponse>(body, JsonOptions);
        return result?.Id;
    }

    private Task<InvoiceWhatsAppResult> SendDocumentAsync(
        string recipient,
        string mediaId,
        string fileName,
        string caption,
        CancellationToken cancellationToken)
        => PostMessageAsync(new
        {
            messaging_product = "whatsapp",
            to = recipient,
            type = "document",
            document = new
            {
                id = mediaId,
                filename = fileName,
                caption
            }
        }, cancellationToken);

    private Task<InvoiceWhatsAppResult> SendTextAsync(
        string recipient,
        string body,
        CancellationToken cancellationToken)
        => PostMessageAsync(new
        {
            messaging_product = "whatsapp",
            to = recipient,
            type = "text",
            text = new
            {
                preview_url = false,
                body
            }
        }, cancellationToken);

    private async Task<InvoiceWhatsAppResult> PostMessageAsync(object payload, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Post, "messages");
        request.Content = JsonContent.Create(payload, options: JsonOptions);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return response.IsSuccessStatusCode
            ? new InvoiceWhatsAppResult(true, null)
            : new InvoiceWhatsAppResult(false, ReadProviderError(body));
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string resource)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_config.WhatsApp.GraphApiBaseUrl)
            ? "https://graph.facebook.com"
            : _config.WhatsApp.GraphApiBaseUrl.TrimEnd('/');
        var apiVersion = string.IsNullOrWhiteSpace(_config.WhatsApp.ApiVersion)
            ? "v24.0"
            : _config.WhatsApp.ApiVersion.Trim('/');
        var phoneNumberId = _config.WhatsApp.PhoneNumberId.Trim();

        var request = new HttpRequestMessage(method, $"{baseUrl}/{apiVersion}/{phoneNumberId}/{resource.TrimStart('/')}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.WhatsApp.AccessToken);
        return request;
    }

    private List<string> BuildRecipients(string? partyPhone)
    {
        var recipients = new HashSet<string>(StringComparer.Ordinal);
        if (_config.WhatsApp.SendToPartyPhone)
        {
            AddPhone(recipients, partyPhone);
        }

        foreach (var recipient in _config.WhatsApp.Recipients)
        {
            AddPhone(recipients, recipient);
        }

        return recipients.ToList();
    }

    private void AddPhone(HashSet<string> recipients, string? value)
    {
        var normalized = NormalizePhone(value);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            recipients.Add(normalized);
        }
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

    private static string BuildCaption(Invoice invoice, string partyName)
        => $"Dear {partyName}, invoice {invoice.InvoiceNumber} for {invoice.TotalAmount:N2} has been generated by Kaj International.";

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

    private sealed class MediaUploadResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}
