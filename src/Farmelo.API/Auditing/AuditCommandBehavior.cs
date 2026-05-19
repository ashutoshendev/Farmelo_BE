using Farmelo.Business.Commands.Owners;
using Farmelo.Business.Commands.Business;
using Farmelo.Business.Commands.Products;
using Farmelo.Shared.DTO.Business;
using Farmelo.Data.Read.Infrastructure;
using Farmelo.Shared.DTO.Owners;
using Farmelo.Shared.DTO.Products;
using MediatR;
using System.Text.Json;

namespace Farmelo.API.Auditing;

public sealed class AuditCommandBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAuditLogger _auditLogger;
    private readonly IDapperExecutor _dapper;

    public AuditCommandBehavior(IAuditLogger auditLogger, IDapperExecutor dapper)
    {
        _auditLogger = auditLogger;
        _dapper = dapper;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var before = await CaptureBeforeAsync(request, cancellationToken);
        var response = await next();

        if (!TryGetSuccessfulPayload(response, out var payload) || payload == null)
        {
            return response;
        }

        AuditSuccessfulRequest(request, payload, before);
        return response;
    }

    private async Task<object?> CaptureBeforeAsync(TRequest request, CancellationToken cancellationToken)
        => request switch
        {
            UpdateOwnerCommand command => await GetOwnerSnapshotAsync(command.OwnerId, cancellationToken),
            DeactivateOwnerCommand command => await GetOwnerSnapshotAsync(command.OwnerId, cancellationToken),
            UpdateProductCommand command => await GetProductSnapshotAsync(command.ProductId, cancellationToken),
            _ => null
        };

    private async Task<OwnerAuditSnapshot?> GetOwnerSnapshotAsync(int ownerId, CancellationToken cancellationToken)
        => await _dapper.QuerySingleAsync<OwnerAuditSnapshot>(
            """
            SELECT Id, FullName, Email, IsActive
            FROM UserAccounts
            WHERE Id = @OwnerId AND Role = 'Owner'
            """,
            new { OwnerId = ownerId },
            ct: cancellationToken);

    private async Task<ProductAuditSnapshot?> GetProductSnapshotAsync(int productId, CancellationToken cancellationToken)
        => await _dapper.QuerySingleAsync<ProductAuditSnapshot>(
            """
            SELECT Id, Slug, Name, Description, Weight, WeightGrams, CurrentPrice, Currency, Ingredients, Nutrition,
                   IsBestseller, IsActive, ImageUrl, AccentColor, BackgroundColor
            FROM Products
            WHERE Id = @ProductId
            """,
            new { ProductId = productId },
            ct: cancellationToken);

    private void AuditSuccessfulRequest(TRequest request, object payload, object? before)
    {
        switch (request)
        {
            case CreateOwnerCommand when payload is OwnerDto owner:
                LogOwnerCreated(owner);
                break;

            case UpdateOwnerCommand when payload is OwnerDto owner:
                LogOwnerUpdated(owner, before as OwnerAuditSnapshot);
                break;

            case DeactivateOwnerCommand when payload is OwnerDto owner:
                LogOwnerStatusChanged(owner, before as OwnerAuditSnapshot);
                break;

            case CreateProductCommand when payload is ProductDto product:
                LogProductCreated(product);
                break;

            case UpdateProductCommand when payload is ProductDto product:
                LogProductUpdated(product, before as ProductAuditSnapshot);
                break;

            case CreatePartyCommand when payload is PartyDto party:
                LogBusinessCreated("Parties", party.Id, party.Name, ToJson(ToDictionary(party)));
                break;

            case UpdatePartyCommand when payload is PartyDto party:
                LogBusinessCreated("Parties", party.Id, party.Name, ToJson(ToDictionary(party)), "UPDATED");
                break;

            case AddRawStockCommand when payload is RawStockEntryDto rawStock:
                LogBusinessCreated("Inventory", rawStock.Id, "Raw stock", ToJson(ToDictionary(rawStock)));
                break;

            case CreateBoxProductionCommand when payload is BoxStockSummaryDto boxStock:
                LogBusinessCreated("Inventory", boxStock.ProductId, boxStock.ProductName, ToJson(ToDictionary(boxStock)));
                break;

            case CreateB2BOrderCommand when payload is B2BOrderDto b2BOrder:
                LogBusinessCreated("B2B", b2BOrder.Id, b2BOrder.PartyName, ToJson(ToDictionary(b2BOrder)));
                break;

            case CreateB2CAssignmentCommand when payload is B2CAssignmentDto b2CAssignment:
                LogBusinessCreated("B2C", b2CAssignment.Id, b2CAssignment.PartyName, ToJson(ToDictionary(b2CAssignment)));
                break;

            case ReturnB2CStockCommand when payload is B2CAssignmentDto b2CAssignment:
                LogBusinessCreated("B2C", b2CAssignment.Id, b2CAssignment.PartyName, ToJson(ToDictionary(b2CAssignment)), "UPDATED");
                break;

            case CreatePaymentCommand when payload is PaymentDto payment:
                LogBusinessCreated("Payments", payment.Id, payment.PartyName, ToJson(ToDictionary(payment)));
                break;
        }
    }

    private void LogBusinessCreated(string module, int targetId, string targetLabel, string newValue, string actionType = "CREATED")
        => _auditLogger.Log(new AuditEvent
        {
            ActionType = actionType,
            Module = module,
            TargetId = targetId,
            TargetLabel = targetLabel,
            OldValue = "{}",
            NewValue = newValue
        });

    private void LogOwnerCreated(OwnerDto owner)
        => _auditLogger.Log(new AuditEvent
        {
            ActionType = "CREATED",
            Module = "Users",
            TargetId = owner.Id,
            TargetLabel = owner.FullName,
            OldValue = "{}",
            NewValue = ToJson(ToOwnerDictionary(owner))
        });

    private void LogOwnerUpdated(OwnerDto owner, OwnerAuditSnapshot? before)
    {
        var oldValues = before == null ? new Dictionary<string, object?>() : ToOwnerDictionary(before);
        var newValues = ToOwnerDictionary(owner);
        var (oldDiff, newDiff) = BuildDiff(oldValues, newValues);

        _auditLogger.Log(new AuditEvent
        {
            ActionType = "UPDATED",
            Module = "Users",
            TargetId = owner.Id,
            TargetLabel = owner.FullName,
            OldValue = ToJson(oldDiff),
            NewValue = ToJson(newDiff)
        });
    }

    private void LogOwnerStatusChanged(OwnerDto owner, OwnerAuditSnapshot? before)
    {
        var oldValues = before == null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?> { ["IsActive"] = before.IsActive };
        var newValues = new Dictionary<string, object?> { ["IsActive"] = owner.IsActive };

        _auditLogger.Log(new AuditEvent
        {
            ActionType = "STATUS_CHANGED",
            Module = "Users",
            TargetId = owner.Id,
            TargetLabel = owner.FullName,
            OldValue = ToJson(oldValues),
            NewValue = ToJson(newValues)
        });
    }

    private void LogProductCreated(ProductDto product)
        => _auditLogger.Log(new AuditEvent
        {
            ActionType = "CREATED",
            Module = "Pricing",
            TargetId = product.Id,
            TargetLabel = product.Name,
            OldValue = "{}",
            NewValue = ToJson(ToProductDictionary(product))
        });

    private void LogProductUpdated(ProductDto product, ProductAuditSnapshot? before)
    {
        var oldValues = before == null ? new Dictionary<string, object?>() : ToProductDictionary(before);
        var newValues = ToProductDictionary(product);
        var (oldDiff, newDiff) = BuildDiff(oldValues, newValues);

        _auditLogger.Log(new AuditEvent
        {
            ActionType = "UPDATED",
            Module = "Pricing",
            TargetId = product.Id,
            TargetLabel = product.Name,
            OldValue = ToJson(oldDiff),
            NewValue = ToJson(newDiff)
        });
    }

    private static bool TryGetSuccessfulPayload(TResponse response, out object? payload)
    {
        payload = null;

        if (response == null)
        {
            return false;
        }

        var responseType = response.GetType();
        var success = responseType.GetProperty("Success")?.GetValue(response);

        if (success is not true)
        {
            return false;
        }

        payload = responseType.GetProperty("Payload")?.GetValue(response);
        return true;
    }

    private static (Dictionary<string, object?> OldDiff, Dictionary<string, object?> NewDiff) BuildDiff(
        IReadOnlyDictionary<string, object?> oldValues,
        IReadOnlyDictionary<string, object?> newValues)
    {
        var oldDiff = new Dictionary<string, object?>();
        var newDiff = new Dictionary<string, object?>();

        foreach (var (key, newValue) in newValues)
        {
            oldValues.TryGetValue(key, out var oldValue);

            if (!ValuesEqual(oldValue, newValue))
            {
                oldDiff[key] = oldValue;
                newDiff[key] = newValue;
            }
        }

        return (oldDiff, newDiff);
    }

    private static bool ValuesEqual(object? oldValue, object? newValue)
    {
        if (oldValue == null && newValue == null)
        {
            return true;
        }

        if (oldValue is decimal oldDecimal && newValue is decimal newDecimal)
        {
            return oldDecimal == newDecimal;
        }

        return string.Equals(Convert.ToString(oldValue), Convert.ToString(newValue), StringComparison.Ordinal);
    }

    private static Dictionary<string, object?> ToOwnerDictionary(OwnerDto owner)
        => new()
        {
            ["FullName"] = owner.FullName,
            ["Email"] = owner.Email,
            ["IsActive"] = owner.IsActive
        };

    private static Dictionary<string, object?> ToOwnerDictionary(OwnerAuditSnapshot owner)
        => new()
        {
            ["FullName"] = owner.FullName,
            ["Email"] = owner.Email,
            ["IsActive"] = owner.IsActive
        };

    private static Dictionary<string, object?> ToProductDictionary(ProductDto product)
        => new()
        {
            ["Slug"] = product.Slug,
            ["Name"] = product.Name,
            ["Description"] = product.Description,
            ["Weight"] = product.Weight,
            ["WeightGrams"] = product.WeightGrams,
            ["CurrentPrice"] = product.CurrentPrice,
            ["Currency"] = product.Currency,
            ["Ingredients"] = product.Ingredients,
            ["Nutrition"] = product.Nutrition,
            ["IsBestseller"] = product.IsBestseller,
            ["IsActive"] = product.IsActive,
            ["ImageUrl"] = product.ImageUrl,
            ["AccentColor"] = product.AccentColor,
            ["BackgroundColor"] = product.BackgroundColor
        };

    private static Dictionary<string, object?> ToProductDictionary(ProductAuditSnapshot product)
        => new()
        {
            ["Slug"] = product.Slug,
            ["Name"] = product.Name,
            ["Description"] = product.Description,
            ["Weight"] = product.Weight,
            ["WeightGrams"] = product.WeightGrams,
            ["CurrentPrice"] = product.CurrentPrice,
            ["Currency"] = product.Currency,
            ["Ingredients"] = product.Ingredients,
            ["Nutrition"] = product.Nutrition,
            ["IsBestseller"] = product.IsBestseller,
            ["IsActive"] = product.IsActive,
            ["ImageUrl"] = product.ImageUrl,
            ["AccentColor"] = product.AccentColor,
            ["BackgroundColor"] = product.BackgroundColor
        };

    private static string ToJson(IReadOnlyDictionary<string, object?> values)
        => values.Count == 0 ? "{}" : JsonSerializer.Serialize(values, JsonOptions);

    private static Dictionary<string, object?> ToDictionary(PartyDto party)
        => new()
        {
            ["PartyType"] = party.PartyType,
            ["Name"] = party.Name,
            ["Phone"] = party.Phone,
            ["Location"] = party.Location,
            ["IsActive"] = party.IsActive
        };

    private static Dictionary<string, object?> ToDictionary(RawStockEntryDto stock)
        => new()
        {
            ["QuantityKg"] = stock.QuantityKg,
            ["AvailableKg"] = stock.AvailableKg,
            ["CostPerKg"] = stock.CostPerKg
        };

    private static Dictionary<string, object?> ToDictionary(BoxStockSummaryDto stock)
        => new()
        {
            ["ProductName"] = stock.ProductName,
            ["AvailableQuantity"] = stock.AvailableQuantity,
            ["AverageUnitCost"] = stock.AverageUnitCost
        };

    private static Dictionary<string, object?> ToDictionary(B2BOrderDto order)
        => new()
        {
            ["PartyName"] = order.PartyName,
            ["QuantityKg"] = order.QuantityKg,
            ["TotalValue"] = order.TotalValue,
            ["Margin"] = order.Margin,
            ["PendingAmount"] = order.PendingAmount
        };

    private static Dictionary<string, object?> ToDictionary(B2CAssignmentDto assignment)
        => new()
        {
            ["PartyName"] = assignment.PartyName,
            ["ProductName"] = assignment.ProductName,
            ["Quantity"] = assignment.Quantity,
            ["ReturnedQuantity"] = assignment.ReturnedQuantity,
            ["TotalValue"] = assignment.TotalValue,
            ["PendingAmount"] = assignment.PendingAmount
        };

    private static Dictionary<string, object?> ToDictionary(PaymentDto payment)
        => new()
        {
            ["PartyName"] = payment.PartyName,
            ["PaymentType"] = payment.PaymentType,
            ["Amount"] = payment.Amount,
            ["PaymentMode"] = payment.PaymentMode
        };

    private sealed class OwnerAuditSnapshot
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    private sealed class ProductAuditSnapshot
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public int WeightGrams { get; set; }
        public decimal CurrentPrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Ingredients { get; set; } = string.Empty;
        public string Nutrition { get; set; } = string.Empty;
        public bool IsBestseller { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public string? AccentColor { get; set; }
        public string? BackgroundColor { get; set; }
    }
}
