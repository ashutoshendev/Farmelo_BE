using Farmelo.API.Controllers.Abstractions;
using Farmelo.Data.Write.Abstractions;
using Farmelo.Data.Write.EFContext;
using Farmelo.Data.Write.Entities;
using Farmelo.Shared.Config;
using Farmelo.Shared.DTO.Checkout;
using Farmelo.Shared.OperationResult;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Farmelo.API.Controllers;

[Route("api/checkout")]
public sealed class CheckoutController : ApiBaseController<CheckoutController>
{
    private const decimal DeliveryFee = 49m;
    private const decimal FreeDeliveryThreshold = 499m;
    private const string CalculationVersion = "farmelo-retail-v1";
    private readonly FarmeloDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly PaymentOptions _paymentOptions;

    public CheckoutController(
        ILogger<CheckoutController> logger,
        IMediator mediator,
        FarmeloDbContext dbContext,
        ICurrentUser currentUser,
        ConfigurationOptions config)
        : base(logger, mediator)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _paymentOptions = config.Payments;
    }

    [HttpPost("quote")]
    public async Task<IActionResult> Quote([FromBody] CheckoutQuoteRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<CheckoutQuoteDto>();
        }

        var result = await BuildQuoteAsync(request.Items, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CheckoutCreateOrderRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<CustomerOrderDto>();
        }

        var quoteResult = await BuildQuoteAsync(request.Items, ct);
        if (!quoteResult.Success || quoteResult.Payload == null)
        {
            return BadRequest(ServiceOperationResult.CreateWithFailure<CustomerOrderDto>(quoteResult.Error ?? "Unable to calculate order amount."));
        }

        var quote = quoteResult.Payload;
        if (Math.Round(request.ClientTotalAmount, 2) != quote.TotalAmount)
        {
            return BadRequest(ServiceOperationResult.CreateWithFailure<CustomerOrderDto>(
                "Order total changed. Please review your cart and try again."));
        }

        var paymentMethod = NormalizePaymentMethod(request.PaymentMethod);
        var now = DateTime.UtcNow;
        var order = new CustomerOrder
        {
            OrderNumber = await BuildOrderNumberAsync(ct),
            UserAccountId = _currentUser.UserId,
            OrderDate = now,
            Status = paymentMethod == "UPI" ? "AwaitingPayment" : "Confirmed",
            PaymentStatus = paymentMethod == "UPI" ? "Pending" : "Unpaid",
            PaymentMethod = paymentMethod,
            CustomerName = request.Address.FullName.Trim(),
            Phone = request.Address.Phone.Trim(),
            Email = NullIfWhiteSpace(request.Address.Email),
            AddressLine = request.Address.Address.Trim(),
            City = request.Address.City.Trim(),
            Pincode = request.Address.Pincode.Trim(),
            Notes = NullIfWhiteSpace(request.Address.Notes),
            SubtotalAmount = quote.SubtotalAmount,
            DiscountAmount = quote.DiscountAmount,
            TaxAmount = quote.TaxAmount,
            DeliveryAmount = quote.DeliveryAmount,
            TotalAmount = quote.TotalAmount,
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        };

        foreach (var item in quote.Items)
        {
            order.Items.Add(new CustomerOrderItem
            {
                ProductSlug = item.ProductSlug,
                ProductName = item.ProductName,
                Weight = item.Weight,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal,
                CreatedBy = CurrentUserName(),
                CreatedOn = now
            });
        }

        order.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentMethod = paymentMethod,
            Status = paymentMethod == "UPI" ? "Pending" : "Unpaid",
            Amount = quote.TotalAmount,
            Provider = paymentMethod == "UPI" ? _paymentOptions.UpiProvider : "COD",
            CreatedOnUtc = now,
            CreatedBy = CurrentUserName(),
            CreatedOn = now
        });

        _dbContext.CustomerOrders.Add(order);
        await _dbContext.SaveChangesAsync(ct);
        return Ok(ServiceOperationResult.CreateWithSuccess(ToOrderDto(order), "Order created."));
    }

    [Authorize]
    [HttpPost("orders/{orderId:int}/upi-status")]
    public async Task<IActionResult> UpdateUpiStatus(int orderId, [FromBody] UpiPaymentUpdateRequestDto? request, CancellationToken ct)
    {
        if (request == null)
        {
            return MissingBodyResult<CustomerOrderDto>();
        }

        var order = await _dbContext.CustomerOrders
            .Include(x => x.Items)
            .Include(x => x.PaymentTransactions)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.UserAccountId == _currentUser.UserId, ct);
        if (order == null)
        {
            return NotFound(ServiceOperationResult.CreateWithFailure<CustomerOrderDto>("Order was not found."));
        }

        if (order.PaymentMethod != "UPI")
        {
            return BadRequest(ServiceOperationResult.CreateWithFailure<CustomerOrderDto>("This order is not a UPI order."));
        }

        if (Math.Round(request.Amount, 2) != order.TotalAmount)
        {
            return BadRequest(ServiceOperationResult.CreateWithFailure<CustomerOrderDto>("Payment amount does not match the order total."));
        }

        var now = DateTime.UtcNow;
        var status = NormalizePaymentStatus(request.Status);
        var transaction = order.PaymentTransactions
            .OrderByDescending(x => x.CreatedOnUtc)
            .FirstOrDefault(x => x.PaymentMethod == "UPI");

        if (transaction == null)
        {
            transaction = new PaymentTransaction
            {
                CustomerOrderId = order.Id,
                PaymentMethod = "UPI",
                Amount = order.TotalAmount,
                Provider = _paymentOptions.UpiProvider,
                CreatedOnUtc = now,
                CreatedBy = CurrentUserName(),
                CreatedOn = now
            };
            order.PaymentTransactions.Add(transaction);
        }

        transaction.Status = status;
        transaction.TransactionId = NullIfWhiteSpace(request.TransactionId);
        transaction.ReferenceId = NullIfWhiteSpace(request.ReferenceId);
        transaction.ProviderResponse = NullIfWhiteSpace(request.ProviderResponse);
        transaction.UpdatedOnUtc = now;
        transaction.ModifiedBy = CurrentUserName();
        transaction.ModifiedOn = now;

        order.PaymentStatus = status;
        order.Status = status switch
        {
            "Success" => "Confirmed",
            "Failed" => "PaymentFailed",
            "Cancelled" => "Cancelled",
            _ => "AwaitingPayment"
        };
        order.ModifiedBy = CurrentUserName();
        order.ModifiedOn = now;

        await _dbContext.SaveChangesAsync(ct);
        return Ok(ServiceOperationResult.CreateWithSuccess(ToOrderDto(order), "Payment status updated."));
    }

    private async Task<ServiceOperationResult<CheckoutQuoteDto>> BuildQuoteAsync(
        IReadOnlyList<CheckoutItemRequestDto> requestItems,
        CancellationToken ct)
    {
        var catalog = await BuildCatalogAsync(ct);
        var quoteItems = new List<CheckoutQuoteItemDto>();

        foreach (var requestItem in requestItems)
        {
            var key = CatalogKey(requestItem.ProductSlug, requestItem.Weight);
            if (!catalog.TryGetValue(key, out var catalogItem))
            {
                return ServiceOperationResult.CreateWithFailure<CheckoutQuoteDto>(
                    $"Product {requestItem.ProductSlug} {requestItem.Weight} is not available.");
            }

            var quantity = requestItem.Quantity;
            var lineTotal = Math.Round(catalogItem.UnitPrice * quantity, 2);
            quoteItems.Add(new CheckoutQuoteItemDto
            {
                ProductSlug = catalogItem.Slug,
                ProductName = catalogItem.Name,
                Weight = catalogItem.Weight,
                Quantity = quantity,
                UnitPrice = catalogItem.UnitPrice,
                LineTotal = lineTotal
            });
        }

        var subtotal = Math.Round(quoteItems.Sum(x => x.LineTotal), 2);
        var delivery = subtotal == 0 || subtotal >= FreeDeliveryThreshold ? 0m : DeliveryFee;
        var quote = new CheckoutQuoteDto
        {
            Items = quoteItems,
            SubtotalAmount = subtotal,
            DiscountAmount = 0m,
            TaxAmount = 0m,
            DeliveryAmount = delivery,
            TotalAmount = Math.Round(subtotal + delivery, 2),
            CalculationVersion = CalculationVersion
        };

        return ServiceOperationResult.CreateWithSuccess(quote);
    }

    private async Task<Dictionary<string, CatalogItem>> BuildCatalogAsync(CancellationToken ct)
    {
        var dbProducts = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => new CatalogItem(x.Slug, x.Name, x.Weight, x.CurrentPrice))
            .ToListAsync(ct);

        var catalog = StaticCatalog()
            .Concat(dbProducts)
            .GroupBy(x => CatalogKey(x.Slug, x.Weight), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Last(), StringComparer.OrdinalIgnoreCase);

        return catalog;
    }

    private static IEnumerable<CatalogItem> StaticCatalog()
    {
        string[] slugs = ["cream-and-onion", "chatpata-masala", "fiery-peri-peri", "minty-pudina", "salt-and-pepper"];
        string[] names = ["Cream and Onion", "Chatpata Masala", "Fiery Peri Peri", "Minty Pudina", "Salt and Pepper"];

        for (var index = 0; index < slugs.Length; index++)
        {
            yield return new CatalogItem(slugs[index], names[index], "100g", 160m);
            yield return new CatalogItem(slugs[index], names[index], "200g", 320m);
        }
    }

    private async Task<string> BuildOrderNumberAsync(CancellationToken ct)
    {
        var prefix = $"FML-{DateTime.UtcNow:yyyyMMdd}";
        var count = await _dbContext.CustomerOrders.CountAsync(x => x.OrderNumber.StartsWith(prefix), ct);
        return $"{prefix}-{count + 1:0000}";
    }

    private CustomerOrderDto ToOrderDto(CustomerOrder order)
        => new()
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            Amounts = new CheckoutQuoteDto
            {
                Items = order.Items.Select(x => new CheckoutQuoteItemDto
                {
                    ProductSlug = x.ProductSlug,
                    ProductName = x.ProductName,
                    Weight = x.Weight,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    LineTotal = x.LineTotal
                }).ToList(),
                SubtotalAmount = order.SubtotalAmount,
                DiscountAmount = order.DiscountAmount,
                TaxAmount = order.TaxAmount,
                DeliveryAmount = order.DeliveryAmount,
                TotalAmount = order.TotalAmount,
                CalculationVersion = CalculationVersion
            },
            Payments = order.PaymentTransactions.Select(x => new PaymentTransactionDto
            {
                Id = x.Id,
                PaymentMethod = x.PaymentMethod,
                Status = x.Status,
                Amount = x.Amount,
                TransactionId = x.TransactionId,
                ReferenceId = x.ReferenceId,
                Provider = x.Provider,
                ProviderResponse = x.ProviderResponse,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc
            }).ToList(),
            UpiPayeeAddress = string.IsNullOrWhiteSpace(_paymentOptions.UpiPayeeAddress) ? null : _paymentOptions.UpiPayeeAddress,
            UpiPayeeName = _paymentOptions.UpiPayeeName,
            UpiIntentUrl = order.PaymentMethod == "UPI" && !string.IsNullOrWhiteSpace(_paymentOptions.UpiPayeeAddress)
                ? BuildUpiIntentUrl(order)
                : null
        };

    private string BuildUpiIntentUrl(CustomerOrder order)
    {
        var query = new Dictionary<string, string>
        {
            ["pa"] = _paymentOptions.UpiPayeeAddress,
            ["pn"] = _paymentOptions.UpiPayeeName,
            ["am"] = order.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture),
            ["cu"] = "INR",
            ["tn"] = $"Farmelo order {order.OrderNumber}",
            ["tr"] = order.OrderNumber
        };

        return "upi://pay?" + string.Join("&", query.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));
    }

    private string CurrentUserName()
        => string.IsNullOrWhiteSpace(_currentUser.UserName) ? "SYSTEM" : _currentUser.UserName;

    private static string NormalizePaymentMethod(string value)
        => value.Equals("UPI", StringComparison.OrdinalIgnoreCase) ? "UPI" : "COD";

    private static string NormalizePaymentStatus(string value)
        => value.Equals("Success", StringComparison.OrdinalIgnoreCase)
            ? "Success"
            : value.Equals("Failed", StringComparison.OrdinalIgnoreCase)
                ? "Failed"
                : value.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
                    ? "Cancelled"
                    : "Pending";

    private static string CatalogKey(string slug, string? weight)
        => $"{slug.Trim().ToLowerInvariant()}::{(string.IsNullOrWhiteSpace(weight) ? "100g" : weight.Trim().ToLowerInvariant())}";

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record CatalogItem(string Slug, string Name, string Weight, decimal UnitPrice);
}
