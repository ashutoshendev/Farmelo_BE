using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class PaymentTransaction : BaseEntity
{
    public int Id { get; set; }
    public int CustomerOrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? ReferenceId { get; set; }
    public string? Provider { get; set; }
    public string? ProviderResponse { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public CustomerOrder? CustomerOrder { get; set; }
}
