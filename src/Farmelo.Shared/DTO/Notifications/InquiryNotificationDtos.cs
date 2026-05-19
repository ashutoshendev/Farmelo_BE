namespace Farmelo.Shared.DTO.Notifications;

public sealed class InquiryNotificationRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Source { get; set; }
}

public sealed class InquiryNotificationResponseDto
{
    public bool EmailSent { get; set; }
    public bool WhatsAppSent { get; set; }
    public string? EmailError { get; set; }
    public string? WhatsAppError { get; set; }
}
