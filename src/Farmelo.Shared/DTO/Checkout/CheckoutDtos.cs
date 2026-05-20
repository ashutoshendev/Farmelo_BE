namespace Farmelo.Shared.DTO.Checkout;

public sealed class CheckoutItemRequestDto
{
    public string ProductSlug { get; set; } = string.Empty;
    public string? Weight { get; set; }
    public int Quantity { get; set; }
}

public sealed class CheckoutQuoteRequestDto
{
    public IReadOnlyList<CheckoutItemRequestDto> Items { get; set; } = Array.Empty<CheckoutItemRequestDto>();
}

public sealed class CheckoutQuoteItemDto
{
    public string ProductSlug { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class CheckoutQuoteDto
{
    public IReadOnlyList<CheckoutQuoteItemDto> Items { get; set; } = Array.Empty<CheckoutQuoteItemDto>();
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DeliveryAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "INR";
    public string CalculationVersion { get; set; } = string.Empty;
}

public sealed class CustomerAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public sealed class CheckoutCreateOrderRequestDto
{
    public IReadOnlyList<CheckoutItemRequestDto> Items { get; set; } = Array.Empty<CheckoutItemRequestDto>();
    public CustomerAddressDto Address { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal ClientTotalAmount { get; set; }
}

public sealed class PaymentTransactionDto
{
    public int Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? ReferenceId { get; set; }
    public string? Provider { get; set; }
    public string? ProviderResponse { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}

public sealed class CustomerOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public CheckoutQuoteDto Amounts { get; set; } = new();
    public IReadOnlyList<PaymentTransactionDto> Payments { get; set; } = Array.Empty<PaymentTransactionDto>();
    public string? UpiIntentUrl { get; set; }
    public string? UpiPayeeAddress { get; set; }
    public string? UpiPayeeName { get; set; }
}

public sealed class UpiPaymentUpdateRequestDto
{
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? ReferenceId { get; set; }
    public string? ProviderResponse { get; set; }
}
