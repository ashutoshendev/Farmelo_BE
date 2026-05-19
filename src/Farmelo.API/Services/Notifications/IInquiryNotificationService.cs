using Farmelo.Shared.DTO.Notifications;

namespace Farmelo.API.Services.Notifications;

public interface IInquiryNotificationService
{
    Task<InquiryNotificationResponseDto> SendAsync(
        InquiryNotificationRequestDto request,
        CancellationToken cancellationToken);
}
